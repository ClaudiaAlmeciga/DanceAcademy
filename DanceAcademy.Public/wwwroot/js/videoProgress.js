// Detecta cuándo termina la reproducción del video de una lección (video directo,
// YouTube o Vimeo) y avisa a Blazor vía JS interop para marcar la lección como completada.
window.videoProgress = {
    _currentCleanup: null,

    // Se llama antes de inicializar el detector de una nueva lección, para no
    // dejar listeners/timers de la lección anterior colgados (SPA sin recarga de página).
    dispose: function () {
        if (window.videoProgress._currentCleanup) {
            window.videoProgress._currentCleanup();
            window.videoProgress._currentCleanup = null;
        }
    },

    watchDirectVideo: function (elementId, dotNetRef) {
        window.videoProgress.dispose();

        const el = document.getElementById(elementId);
        if (!el) return;

        const handler = () => dotNetRef.invokeMethodAsync('OnVideoEndedFromJs');
        el.addEventListener('ended', handler);

        window.videoProgress._currentCleanup = () => el.removeEventListener('ended', handler);
    },

    watchYoutube: function (elementId, dotNetRef) {
        window.videoProgress.dispose();

        let player = null;
        let disposed = false;

        function createPlayer() {
            if (disposed) return;
            player = new YT.Player(elementId, {
                events: {
                    onStateChange: function (event) {
                        if (event.data === YT.PlayerState.ENDED) {
                            dotNetRef.invokeMethodAsync('OnVideoEndedFromJs');
                        }
                    }
                }
            });
        }

        if (window.YT && window.YT.Player) {
            createPlayer();
        } else {
            const previousCallback = window.onYouTubeIframeAPIReady;
            window.onYouTubeIframeAPIReady = function () {
                if (typeof previousCallback === 'function') previousCallback();
                createPlayer();
            };
            if (!document.getElementById('youtube-iframe-api')) {
                const tag = document.createElement('script');
                tag.id = 'youtube-iframe-api';
                tag.src = 'https://www.youtube.com/iframe_api';
                document.body.appendChild(tag);
            }
        }

        window.videoProgress._currentCleanup = () => {
            disposed = true;
            if (player && typeof player.destroy === 'function') player.destroy();
        };
    },

    watchVimeo: function (elementId, dotNetRef) {
        window.videoProgress.dispose();

        let player = null;
        let disposed = false;

        function createPlayer() {
            if (disposed) return;
            const el = document.getElementById(elementId);
            if (!el) return;
            player = new Vimeo.Player(el);
            player.on('ended', function () {
                dotNetRef.invokeMethodAsync('OnVideoEndedFromJs');
            });
        }

        if (window.Vimeo && window.Vimeo.Player) {
            createPlayer();
        } else {
            const existingTag = document.getElementById('vimeo-player-api');
            if (existingTag) {
                existingTag.addEventListener('load', createPlayer);
            } else {
                const tag = document.createElement('script');
                tag.id = 'vimeo-player-api';
                tag.src = 'https://player.vimeo.com/api/player.js';
                tag.onload = createPlayer;
                document.body.appendChild(tag);
            }
        }

        window.videoProgress._currentCleanup = () => {
            disposed = true;
            if (player && typeof player.unload === 'function') player.unload();
        };
    }
};
