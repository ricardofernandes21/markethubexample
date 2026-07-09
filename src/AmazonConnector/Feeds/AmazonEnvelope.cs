namespace AmazonConnector.Feeds;

using System.Xml.Serialization;

[XmlRoot("AmazonEnvelope")]
public sealed class AmazonEnvelope
{
    public AmazonEnvelopeHeader Header { get; set; } = new();
    public string MessageType { get; set; } = "Inventory";
    public AmazonFeedMessage Message { get; set; } = new();
}

public sealed class AmazonEnvelopeHeader
{
    public string DocumentVersion { get; set; } = "1.01";
    public string MerchantIdentifier { get; set; } = string.Empty;
}

public sealed class AmazonFeedMessage
{
    public int MessageID { get; set; }
    public string OperationType { get; set; } = "Update";
    public AmazonInventoryItem Inventory { get; set; } = new();
}

public sealed class AmazonInventoryItem
{
    public string SKU { get; set; } = string.Empty;

    [XmlElement("Price")]
    public decimal Price { get; set; }

    [XmlElement("Quantity")]
    public int Quantity { get; set; }
}
