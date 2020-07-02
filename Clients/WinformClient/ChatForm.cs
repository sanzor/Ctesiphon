using PubSubSharp.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace WinformClient {
    public partial class ChatForm : Form {
        private State state;
        public ChatForm(State state) {
            this.state = state;
            InitializeComponent();
            this.Initialize();
        }
        public void Initialize() {
            this.AttachHandlers();
        }
        public void AttachHandlers() {
            this.connectBtn.Click += async (s, e) => {

                if (!await this.ConnectAsync(this.connectTb.Text)) {
                    MessageBox.Show("Could not connect to remote endpoint");
                    return;
                }
                this.state.MessagePipe = Observable.FromAsync(async () => {
                    Memory<byte> memory = ArrayPool<byte>.Shared.Rent(1024);
                    var result= await this.state.Socket.ReceiveAsync(memory, CancellationToken.None);
                    var data = memory.Slice(0, result.Count);
                    return data;
                }).Select(x=> {
                    try {
                        var message = Encoding.UTF8.GetString(x.ToArray());
                        var result = JsonSerializer.Deserialize<ChatMessage>(message);
                        return result;
                    } catch (Exception ex) {

                        return null;
                    }
                }).Where(x=>x!=null).Retry().Repeat();
                this.connectTb.Enabled = false;
                this.disconnectBtn.Enabled = true;
                this.connectBtn.Enabled = false;
                this.connectBtn.Visible = false;
                this.disconnectBtn.Visible = true;
                this.msgTb.Enabled = true;

            };
            this.disconnectBtn.Click += (s, e) => {
                this.connectTb.Enabled = true;
                this.connectBtn.Enabled = true;
                this.connectBtn.Visible = true;
                this.disconnectBtn.Enabled = false;
                this.disconnectBtn.Visible = false;
                this.msgTb.Enabled = false;
            };
            this.quitBtn.Click += (x, e) => this.Close();

            this.subscribeBtn.Click += (x, e) => this.subscribeTb.Text = "";
        }
        private async Task<bool> ConnectAsync(string url, CancellationToken token = default) {
            try {
                await this.state.Socket.ConnectAsync(new Uri(url), token);
                return this.state.Socket.State == System.Net.WebSockets.WebSocketState.Open;
            } catch (Exception ex) {

                return false;
            }
        }

        private void connectTb_Validating(object sender, CancelEventArgs e) {
            if (this.connectTb.Text == "") {
                MessageBox.Show("Url is empty ! Please select an appropriate url !");
                return;
            }
            if (!(this.connectTb.Text.StartsWith("ws://"))) {
                MessageBox.Show("Invalid ws url");
            }
        }
    }
}
