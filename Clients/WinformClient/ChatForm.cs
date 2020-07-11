using Ctesiphon.Models;
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
        private Client client;
        private readonly Dictionary<string, IObservable<ChatMessage>> streamMap = new Dictionary<string, IObservable<ChatMessage>>();
        public ChatForm(Client state) {
            this.client = state;
            InitializeComponent();
            this.Initialize();
        }
        public void Initialize() {
            this.AttachHandlers();
        }
        public void AttachHandlers() {
            this.connectBtn.Click += async (s, e) => {
                
                if (!await this.client.ConnectAsync(this.connectTb.Text)) {
                    MessageBox.Show("Could not connect to remote endpoint");
                    return;
                }

                this.connectTb.Enabled = false;
                this.subscribeBtn.Enabled = true;
                this.disconnectBtn.Enabled = true;
                this.connectBtn.Enabled = false;
                this.connectBtn.Visible = false;
                this.disconnectBtn.Visible = true;
                this.msgTb.Enabled = true;
                this.subscribeTb.Enabled = true;

            };
            this.disconnectBtn.Click += (s, e) => {
                this.connectTb.Enabled = true;
                this.connectBtn.Enabled = true;
                this.subscribeBtn.Enabled = false;
                this.subscribeTb.Enabled = false;
                this.connectBtn.Visible = true;
                this.disconnectBtn.Enabled = false;
                this.disconnectBtn.Visible = false;
                this.msgTb.Enabled = false;
            };
            this.quitBtn.Click += async (x, e) => {
                await this.client.DisconnectAsync();
                this.Close();
            };
            this.subscribeBtn.Click += (s, e) => {

            }

            this.subscribeBtn.Click += (x, e) => this.subscribeTb.Text = "";
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
