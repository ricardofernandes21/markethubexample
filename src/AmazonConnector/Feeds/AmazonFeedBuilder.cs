namespace AmazonConnector.Feeds;

using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Contracts.Messages;

public static class AmazonFeedBuilder
{
    private static readonly XmlSerializer Serializer = new(typeof(AmazonEnvelope));

    public static string BuildInventoryFeed(string merchantId, ProductUpdated product, int messageId = 1)
    {
        var envelope = new AmazonEnvelope
        {
            Header = new AmazonEnvelopeHeader
            {
                MerchantIdentifier = merchantId
            },
            Message = new AmazonFeedMessage
            {
                MessageID = messageId,
                Inventory = new AmazonInventoryItem
                {
                    SKU      = product.SKU,
                    Price    = product.Price,
                    Quantity = product.Stock
                }
            }
        };

        var sb = new StringBuilder();
        var settings = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 };

        using var writer = XmlWriter.Create(sb, settings);
        Serializer.Serialize(writer, envelope);

        return sb.ToString();
    }
}
