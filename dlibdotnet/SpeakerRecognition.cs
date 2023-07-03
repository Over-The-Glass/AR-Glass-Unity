using System.Collections.Generic;
using DlibDotNet;
using NRKernal;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class SpeakerRecognition : MonoBehaviour {

    public Text debugLog;
    public RawImage captureImage;
    public Text speakerName;
    public Button activateButton;

    private bool isActivated = false;
    private FrontalFaceDetector detector;
    private ShapePredictor sp;
    private DlibDotNet.Dnn.LossMetric net;
    private List<Matrix<float>> knownFaces = new List<Matrix<float>>();
    private List<string> knownNames = new List<string>();
    private List<LipMovement> movements = new List<LipMovement>();
    private (double, double) latestSpeakerPosition;
    private List<float> faceDiffs = new List<float>();
    private List<double> lipDiffs = new List<double>(); 
    private NRRGBCamTexture camTexture;
    private Texture2D mTex2d;

    void Start()
    {
        detector = Dlib.GetFrontalFaceDetector();
        sp = ShapePredictor.Deserialize(Application.dataPath + "/Plugins/shape_predictor_68_face_landmarks.dat");
        net = DlibDotNet.Dnn.LossMetric.Deserialize(Application.dataPath + "/Plugins/dlib_face_recognition_resnet_model_v1.dat");

		LoadKnownFaces();

		camTexture = new NRRGBCamTexture();
        captureImage.texture = camTexture.GetTexture();
        mTex2d = camTexture.GetTexture();
        
        activateButton.onClick.AddListener(ToggleActivation);
    }

    private IEnumerator RecognizeSpeaker() {
        while(isActivated){
            if(!isActivated || camTexture == null)
            {
                break;
            }

			speakerName.text = "?";

            var temp = mTex2d.GetRawTextureData();

            var array = new byte[temp.Length];
            
            var cimg = Dlib.LoadImageData<RgbPixel>(temp, (uint)mTex2d.height, (uint)mTex2d.width, (uint)(mTex2d.width * 3));
	
			Matrix<RgbPixel> img= new Matrix<RgbPixel>(cimg);
	
            debugLog.text = "loaded image data";

            var faces = new List<Matrix<RgbPixel>>();
            var shapes = new List<FullObjectDetection>();

            foreach(var face in detector.Operator(img))
            {
                var shape = sp.Detect(img, face);
                var faceChipDetail = Dlib.GetFaceChipDetails(shape, 150, 0.25);
                var faceChip = Dlib.ExtractImageChip<RgbPixel>(img, faceChipDetail);

                shapes.Add(shape);
                faces.Add(faceChip);
            }

            debugLog.text = ("face num : " + faces.Count);
            

            var faceDescriptors = net.Operator(faces);

            for(int i = 0; i < faceDescriptors.Count; i++)
            {
				 faceDiffs.Clear();

                 for(int j = 0; j < knownFaces.Count; j++)
                 {
                     var diff = Dlib.Length(knownFaces[j] - faceDescriptors[i]);
                     faceDiffs.Add(diff);
                 }

                 var minDiff = faceDiffs.Min();
                 var index = faceDiffs.IndexOf(minDiff);
                
                 var matchRate = 1 / (1 + minDiff);
                 if(matchRate > 0.5)
                 {
                     var eyeDistance = (shapes[i].GetPart(37) - shapes[i].GetPart(44)).Length;
                     var lipHeight = (shapes[i].GetPart(62) - shapes[i].GetPart(66)).Length;
                     var lipWidth = (shapes[i].GetPart(48) - shapes[i].GetPart(54)).Length;

                     (double, double) averageLength = movements[index].CheckMovement(lipWidth / eyeDistance * 100, lipHeight / eyeDistance * 100);
                     if(averageLength.Item1 > 2 || averageLength.Item2 > 2)
                     {
                         lipDiffs[index] = averageLength.Item1 + averageLength.Item2;
                     }
                 }

            }
            speakerName.text = knownNames[lipDiffs.IndexOf(lipDiffs.Min())];
        }
        yield return null;
    }

	public void LoadKnownFaces()
	{
		debugLog.text = "load known faces";

		string relativePath = "NRSDK/Demos/OverTheGlass/Scripts/faces";
		string filePath = Application.dataPath + "/" + relativePath;

		foreach (var file in System.IO.Directory.GetFiles(filePath, "*.jpg"))
		{
			var img = Dlib.LoadImageAsMatrix<RgbPixel>(file);
			var faces = detector.Operator(img);

			if (faces.Any())
			{
				// Detect face
				var shape = sp.Detect(img, faces[0]);
				var faceChipDetail = Dlib.GetFaceChipDetails(shape, 150, 0.25);
				var faceChip = Dlib.ExtractImageChip<RgbPixel>(img, faceChipDetail);

				// Get face descriptor
				var faceDescriptor = net.Operator(faceChip);

				// Add known face and name informations
				knownFaces.Add(faceDescriptor.First());
				knownNames.Add(System.IO.Path.GetFileNameWithoutExtension(file));
			}
			else
			{
				Debug.Log("No face found in " + file);
			}
		}
		lipDiffs = new List<double>(knownFaces.Count);
		if(knownNames != null)
		{
			debugLog.text = knownNames[0];
		}
		else
		{
			debugLog.text = "couldn't load known faces";
		}
		return;
	}

	internal class LipMovement
    {
        private string name;
        private System.Collections.Generic.Queue<double> widthDiffs;
        private System.Collections.Generic.Queue<double> heightDiffs;
        private double prevHeight;
        private double prevWidth;

        public LipMovement(string name)
        {
            this.name = name;
            widthDiffs = new System.Collections.Generic.Queue<double>(3);
            heightDiffs = new System.Collections.Generic.Queue<double>(3);
            prevHeight = 0;
            prevWidth = 0;
        }

        public (double, double) CheckMovement(double width, double height)
        {
            heightDiffs.Enqueue(Math.Abs(prevHeight - height));
            widthDiffs.Enqueue(Math.Abs(prevWidth - width));

            List<double> widthNumbers = new List<double>(widthDiffs);
            List<double> heightNumbers = new List<double>(heightDiffs);

            double widthAverage = widthNumbers.Sum() / widthNumbers.Count;
            double heightAverage = heightNumbers.Sum() / heightNumbers.Count;

            prevHeight = height;
            prevWidth = width;

            return (Math.Round(widthAverage, 3), Math.Round(heightAverage, 3));
        }
    }

    public void ToggleActivation()
    {
        isActivated = !isActivated;

        if (isActivated)
        {
            if(camTexture == null)
            {
                camTexture = new NRRGBCamTexture();
                captureImage.texture = camTexture.GetTexture();
            }
            camTexture.Play();
            captureImage.texture = camTexture.GetTexture();
            mTex2d = camTexture.GetTexture();

            StartCoroutine(RecognizeSpeaker());
        }
        else
        {
            camTexture?.Stop();
            camTexture = null;
            StopCoroutine(RecognizeSpeaker());
        }
    }
    void OnDestroy()
    {
        camTexture?.Stop();
        camTexture = null;
    }
}
