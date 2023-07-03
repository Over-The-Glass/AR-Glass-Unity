using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using NRKernal;
using System.Collections;

public class CaptureAndSend : MonoBehaviour
{
	public RawImage captureImage; // 카메라 이미지를 표시할 RawImage 컴포넌트
	public Button activeButton; // 데이터 전송을 시작/정지할 버튼
	private NRRGBCamTexture rgbCamTexture; // 카메라 텍스처
	private bool isSendingData; // 데이터 전송 여부
	public Button urlBtn; // URL을 설정할 버튼
	public InputField urlInputField; // URL을 입력할 InputField 컴포넌트
	private string url; // web request를 보낼 URl
	private Texture2D texture2d;

	private void Start()
	{
		rgbCamTexture = new NRRGBCamTexture();
		captureImage.texture = rgbCamTexture.GetTexture();
		texture2d = rgbCamTexture.GetTexture();
		isSendingData = false;
		activeButton.onClick.AddListener(ToggleSendingData);
		urlBtn.onClick.AddListener(() => {
			// 빈 문자열이 아닐 경우 작동
			if (!string.IsNullOrEmpty(urlInputField.text))
			{
				url = urlInputField.text + "/camera";
			}
		});
	}

	private IEnumerator SendCameraData()
	{
		while (true)
		{
			if (!isSendingData)
			{
				yield break;
			}
			yield return new WaitForEndOfFrame();

			// 원시 이미지 데이터
			byte[] frameData = texture2d.EncodeToJPG(25);
			StartCoroutine(PostFrameData(frameData));

			yield return null;
		}
	}


	private IEnumerator PostFrameData(byte[] data)
	{
		// UnityWebRequest 생성 및 메서드를 POST로 설정
		UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);

		// 데이터와 컨텐츠 타입 설정
		request.uploadHandler = new UploadHandlerRaw(data);
		request.uploadHandler.contentType = "image/jpeg"; // 원시 이미지 데이터의 유형 설정

		// 요청 보내고 응답 대기
		yield return request.SendWebRequest();

		// 오류 확인
		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError("Error sending camera data: " + request.error);
		}

		// 요청 정리
		request.Dispose();

		yield return null; // 텍스쳐 적용 이후에 다음 프레임으로 넘어가도록 추가
	}

	public void ToggleSendingData()
	{
		isSendingData = !isSendingData;

		if (isSendingData)
		{
			// 데이터 전송 시작
			if (rgbCamTexture == null)
			{
				rgbCamTexture = new NRRGBCamTexture();
				captureImage.texture = rgbCamTexture.GetTexture();
			}
			rgbCamTexture.Play();
			captureImage.texture = rgbCamTexture.GetTexture();
			texture2d = rgbCamTexture.GetTexture();
			StartCoroutine(SendCameraData());
		}
		else
		{
			// 데이터 전송 정지
			rgbCamTexture?.Stop();
			StopCoroutine(SendCameraData()); // SendCameraData 코루틴 정지
		}
	}

	private void OnDestroy()
	{
		rgbCamTexture?.Stop();
	}
}
