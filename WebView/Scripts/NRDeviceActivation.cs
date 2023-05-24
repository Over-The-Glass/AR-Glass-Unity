using UnityEngine;
using Nreal;
using NRKernal;

public class NRDeviceActivation : MonoBehaviour
{
    private bool conversationModeActivated = false; // 대화 모드 활성화 여부를 추적하는 변수

    void Start()
    {
        // WebView 페이지 로드 완료 이벤트를 구독
        WebViewManager.OnPageLoadComplete += OnPageLoadComplete;
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        WebViewManager.OnPageLoadComplete -= OnPageLoadComplete;
    }

    // WebView 페이지 로드 완료 시 호출되는 콜백 함수
    void OnPageLoadComplete(string url)
    {
        if (url.Contains("/conversation-mode"))
        {
            // /conversation-mode 페이지에 접속한 경우
            conversationModeActivated = true;
            ActivateNRDevice(); // Nreal.NRDevice 활성화
        }
        else
        {
            // 다른 페이지에 접속한 경우
            conversationModeActivated = false;
            DeactivateNRDevice(); // Nreal.NRDevice 비활성화
        }
    }

    // Nreal.NRDevice 활성화
    void ActivateNRDevice()
    {
        if (conversationModeActivated)
        {
            NRDevice.Instance.Initialize(); // NRDevice 초기화

            // Nreal Light의 카메라 활성화
            NRSessionManager.Instance.StartHMDTracking();

            // Nreal Light의 마이크 활성화
            NRSessionManager.Instance.SetAudioState(true);
        }
    }

    // Nreal.NRDevice 비활성화
    void DeactivateNRDevice()
    {
        if (!conversationModeActivated)
        {
            // Nreal Light의 카메라 비활성화
            NRSessionManager.Instance.StopHMDTracking();

            // Nreal Light의 마이크 비활성화
            NRSessionManager.Instance.SetAudioState(false);
        }
    }
}
