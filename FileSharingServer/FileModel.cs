﻿namespace FileSharingServer;

public class FileModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Sender { get; set; }
    public byte[] Content { get; set; }

}