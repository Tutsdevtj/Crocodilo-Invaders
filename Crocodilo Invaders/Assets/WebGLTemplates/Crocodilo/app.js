(() => {
  const canvas = document.getElementById("unity-canvas");
  const loading = document.getElementById("loading");
  const error = document.getElementById("error");
  const progressBar = document.getElementById("progress-bar");
  const progressText = document.getElementById("progress-text");
  const fullscreen = document.getElementById("fullscreen");
  const { loaderUrl, config } = window.crocodiloBuild;

  canvas.addEventListener("pointerdown", () => canvas.focus());

  const loader = document.createElement("script");
  loader.src = loaderUrl;
  loader.onload = () => {
    createUnityInstance(canvas, config, progress => {
      const percent = Math.round(progress * 100);
      progressBar.style.width = `${percent}%`;
      progressText.textContent = `${percent}%`;
    }).then(instance => {
      loading.hidden = true;
      fullscreen.disabled = false;
      fullscreen.addEventListener("click", () => instance.SetFullscreen(1));
      canvas.focus();
    }).catch(message => {
      loading.hidden = true;
      error.hidden = false;
      error.textContent = `Não foi possível carregar o jogo: ${message}`;
    });
  };
  loader.onerror = () => {
    loading.hidden = true;
    error.hidden = false;
    error.textContent = "Não foi possível carregar os arquivos do WebGL. Confira se a página está em um servidor HTTP e se a pasta Build foi publicada junto.";
  };
  document.body.appendChild(loader);
})();
