// WebGL 전용 — 보안 레이싱 HTML을 DOM iframe으로 띄우는 브릿지.
// RacingWebViewBridge.cs가 DllImport("__Internal")로 호출한다.
mergeInto(LibraryManager.library, {

  RacingIframe_Show: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    var f = document.getElementById('racing-iframe');
    if (!f) {
      f = document.createElement('iframe');
      f.id = 'racing-iframe';
      f.setAttribute('allow', 'autoplay');
      f.style.cssText =
        'position:fixed;left:50%;top:50%;transform:translate(-50%,-50%);' +
        'width:min(1280px,95vw);height:min(720px,90vh);' +
        'border:2px solid #a78bfa;background:#000;z-index:9999;';
      document.body.appendChild(f);

      // 레이싱 HTML → postMessage({type:'securityRaceFinished', rank}) 수신 (최초 1회 등록)
      window.addEventListener('message', function (e) {
        var d = e.data;
        if (d && d.type === 'securityRaceFinished') {
          try { SendMessage('RacingWebViewBridge', 'OnRaceFinishedFromJs', d.rank | 0); }
          catch (err) { console.warn('[RacingIframe] SendMessage 실패:', err); }
        }
      });
    }
    f.src = url;
    f.style.display = 'block';
    setTimeout(function () { try { f.contentWindow.focus(); } catch (e) {} }, 150);
  },

  RacingIframe_Hide: function () {
    var f = document.getElementById('racing-iframe');
    if (f) {
      f.style.display = 'none';
      f.src = 'about:blank';
    }
    // 포커스를 Unity 캔버스로 복귀 (키 입력 재개)
    try {
      var c = document.querySelector('#unity-canvas') || document.querySelector('canvas');
      if (c) c.focus();
    } catch (e) {}
  }
});
