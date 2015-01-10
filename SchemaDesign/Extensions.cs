using System;
using System.Globalization;
using System.Xml.Linq;

namespace SchemaDesign
{
	public static class Extensions
	{
		public static bool ParseBool(this XElement element, string attributeName)
		{
			var attribute = element.Attribute(attributeName);
			return attribute != null && string.Equals(attribute.Value, "true", StringComparison.InvariantCultureIgnoreCase);
		}

		public static int ParseInt(this XElement element, string attributeName)
		{
			var attribute = element.Attribute(attributeName);
			if (attribute != null)
			{
				int value;
				if (int.TryParse(attribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
				{
					return value;
				}
			}
			return 0;
		}
	}
}
