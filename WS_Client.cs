using WebSocketSharp;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using NRKernal;

public class WS_Client : MonoBehaviour
{
    public TMP_Text ScriptTxt;
    public string msg;
    WebSocket ws;
    void Start()
    {
        ws = new WebSocket("ws://localhost:8080");
        ws.OnMessage += (sender, e)=>{
            msg = System.Text.Encoding.ASCII.GetString(e.RawData);
            Debug.Log("Message received from "+((WebSocket)sender).Url+", Data : " + msg);
        };
        ws.Connect();    
    }

    void Update()
    {
        if(ws == null){
            return;
        }

        if (NRInput.GetButtonDown(ControllerButton.TRIGGER)){
            ws.Send("Hello");
            ScriptTxt.SetText(msg);
        }
        
    }
}
