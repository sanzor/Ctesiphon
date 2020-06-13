using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformClient {
    public partial class ChatForm : Form {
        private ConcurrentHashSet state;
        public ChatForm(ConcurrentHashSet state) {
            this.state = state;
            InitializeComponent();
        }
        public void AttachHandlers() {
            this.quitBtn.Click += (x, e) => this.Close();
            this.subscribeBtn.Click += (x, e) =>this.subscribeTxtBox.Text;
        }

    }
}
