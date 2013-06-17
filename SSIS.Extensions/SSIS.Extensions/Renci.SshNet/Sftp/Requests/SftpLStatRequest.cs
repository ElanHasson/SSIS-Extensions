﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp.Requests
{
    internal class SftpLStatRequest : SftpRequest
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.LStat; }
        }

        public string Path { get; private set; }

        public Encoding Encoding { get; private set; }

        public SftpLStatRequest(uint protocolVersion, uint requestId, string path, Encoding encoding, Action<SftpAttrsResponse> attrsAction, Action<SftpStatusResponse> statusAction)
            : base(protocolVersion, requestId, statusAction)
        {
            this.Path = path;
            this.Encoding = encoding;
            this.SetAction(attrsAction);
        }

        protected override void LoadData()
        {
            base.LoadData();
            this.Path = this.ReadString(this.Encoding);
        }

        protected override void SaveData()
        {
            base.SaveData();
            this.Write(this.Path, this.Encoding);
        }
    }
}
