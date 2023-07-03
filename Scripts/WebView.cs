using UnityEngine;
using UnityEngine.UI;

public class WebView : MonoBehaviour {

  public GameObject baseControllerPanel;
  public Button baseControllerBtn;
  public Button webAppBtn;
  public Button forwardBtn;
  public Button goBackBtn;
  public Button urlBtn;
  public InputField urlInputField;
  public Text subtitle;
  public Text speakerName;

  private WebViewObject webViewObject;
  private string strUrl = "https://naver.com";
  
  void Start () {
    SetButtons();
    StartWebView();
  }

  // initialize web view object
  public void StartWebView()
  {
    webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
    webViewObject.Init(
      // 웹페이지로부터 전달된 데이터를 받습니다.
      cb: (msg) =>
      {
        Debug.Log(string.Format("CallFromJS[{0}]", msg));
		  string[] parts = msg.Split(',');
		  speakerName.text = parts[0];
		  subtitle.text = parts[1];
	  },
      ld: (msg) => {
        Debug.Log($"WebView Loaded : {msg}");
        // webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");                    
      }, androidForceDarkMode: 1  // 0: follow system setting, 1: force dark off, 2: force dark on
    );

    webViewObject.LoadURL(strUrl);
    webViewObject.SetVisibility(true);
    webViewObject.SetMargins(0, 0, 0, Screen.height / 5);
    webViewObject.SetAlertDialogEnabled(true);
    webViewObject.SetCameraAccess(true);
    webViewObject.SetMicrophoneAccess(true);
  }

  // set functions of buttons
  public void SetButtons(){
    baseControllerBtn.onClick.AddListener(() => { 
      webViewObject.SetVisibility(false);
      baseControllerPanel.SetActive(true);
    });

    webAppBtn.onClick.AddListener(() => { 
      webViewObject.SetVisibility(true);
      baseControllerPanel.SetActive(false);
    });

    forwardBtn.onClick.AddListener(() => {
      webViewObject.GoForward();
    });

    goBackBtn.onClick.AddListener(() => {
      webViewObject.GoBack();
    });

    urlBtn.onClick.AddListener(() => {
      strUrl = urlInputField.text;
      webViewObject.LoadURL(strUrl);
    });
  }
}

