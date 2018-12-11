using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace nosale.stackoverflow.WPF
{
    /// <summary>
    /// <see cref="MarkupExtension"/> to create a new <see cref="ControlTemplate"/> based on default <see cref="ControlTemplate"/>,
    /// where 'VerticalAlignment' and 'HorizontalAlignment' of <see cref="ContentPresenter"/> are bind,
    /// so that it's possible to use 'VerticalContentAlignment' and 'HorizontalContentAlignment' for alignment
    /// </summary>
    public class AlignmentControlTemplateExtension : MarkupExtension
    {
        private Type _type;

        public Type Type
        {
            get => _type;
            set
            {
                if(!typeof(Control).IsAssignableFrom(value))
                    throw new ArgumentException($"Type '{value}' is not of type '{typeof(Control)}'");
                _type = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return CreateContentAlignmentControlTemplate(Type);
        }

        static ControlTemplate CreateContentAlignmentControlTemplate(Type type)
        {
            var controlTemplateXDocument = GetDefaultControlTemplate(type);

            var contentPresenter = controlTemplateXDocument.XPathSelectElement($"//*[local-name()='{nameof(ContentPresenter)}']");

            //Set 'TemplateBinding' on 'VerticalAlignment' and 'HorizontalAlignment'
            SetAttributeValue(contentPresenter, nameof(Control.VerticalAlignment), $"{{TemplateBinding {nameof(Control.VerticalContentAlignment)}}}");
            SetAttributeValue(contentPresenter, nameof(Control.HorizontalAlignment), $"{{TemplateBinding {nameof(Control.HorizontalContentAlignment)}}}");

            //Create new 'ControlTemplate' from modified 'XDocument'
            using (StringWriter stringWriter = new StringWriter())
            {
                controlTemplateXDocument.Save(stringWriter);
                return (ControlTemplate)XamlReader.Parse(stringWriter.ToString());
            }
        }

        /// <summary>
        /// Returns default <see cref="ControlTemplate"/> of passed type as <see cref="XDocument"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static XDocument GetDefaultControlTemplate(Type type)
        {
            //Get default 'Style'
            var style = (Style)Application.Current.FindResource(type);

            //Get 'Setter' for 'ControlTemplate'
            var controlTemplateSetter = (Setter)style.Setters.First(setterBase =>
                setterBase is Setter setter && Equals(setter.Property, Control.TemplateProperty));

            var defaultControlTemplate = (ControlTemplate) controlTemplateSetter.Value;

            //Serialize default 'ControlTemplate' to xml/xaml and load into 'XDocument'
            using (StringWriter stringWriter = new StringWriter())
            {
                XamlWriter.Save(defaultControlTemplate, stringWriter);
                using (StringReader reader = new StringReader(stringWriter.ToString()))
                {
                    var xmlReader = XmlReader.Create(reader);
                    return XDocument.Load(xmlReader);
                }
            }
        }

        /// <summary>
        /// Sets or overwrites attribute value
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <param name="value"></param>
        static void SetAttributeValue(XElement element, string attributeName, string value)
        {
            var attribute = element.Attribute(attributeName);
            if (attribute != null)
            {
                attribute.Value = value;
            }
            else
            {
                element.Add(new XAttribute(attributeName, value));
            }
        }


    }
}
