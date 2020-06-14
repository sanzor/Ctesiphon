using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PubSubSharp.Extensions;

namespace WinformClient {
    //public class SocketObservable<T>:IObservable<T> {
    //    private Task loopTask;
    //    private ClientWebSocket connectedSocket;
    //    private IObservable<T> observable;
    //    public SocketObservable(ClientWebSocket socket,CancellationToken token) {
    //        this.connectedSocket = socket;
    //    }
    //    public static SocketObservable<T> CreateObservable<T>(ClientWebSocket socket, CancellationToken token = default) {
    //        SocketObservable<T> socketObs = new SocketObservable<T>(socket,token);
    //        socketObs.loopTask = Task.Run(()=>socketObs.LoopAsync(token),token);
    //        Observable.Using(()=>socketObs.connectedSocket)
    //    }
    //    private async Task LoopAsync(CancellationToken token = default) {
    //        this
    //        while (true) {
    //            token.ThrowIfCancellationRequested();
                
    //        }
    //    }
    //    public IDisposable Subscribe(IObserver<T> observer) {
    //        if (connectedSocket.State != WebSocketState.Open) {
    //            throw new NotSupportedException();
    //        }

    //    }
    //}
}
