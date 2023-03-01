const WebSocket = require('ws');
const wss = new WebSocket.Server({port: 8080}, ()=>{
    console.log('server started');
});

wss.on('connection', (ws)=>{
    
    ws.send('sever connected');
    ws.on('message', (data)=>{
        
        console.log('data received %o', data)
        ws.send(data)
    });
});

wss.on('listening', ()=>{
    console.log('server is listening on port 8080');
});