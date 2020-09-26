using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net;

namespace PowerView.Configuration.Migration
{
  public class ConfigMigrater
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly IList<ISpec> specs = new List<ISpec>
    {
//      new RemoveSpec("configuration/configSections/section[@name='Register']"),
//      new RemoveSpec("configuration/Register"),
    };

    private bool dirty = false;
    
    public void Migrate(string filePath)
    {
      var root = Load(filePath);

      ApplySpecs(root);

      Save(filePath, root);
    }

    private XmlDocument Load(string filePath)
    {
      var xmlDocument = new XmlDocument { PreserveWhitespace = true };
      using (var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        xmlDocument.Load(file);
      }
      return xmlDocument;
    }

    private void Save(string filePath, XmlDocument xmlDocument)
    {
      if (!dirty)
      {
        return;
      }

      log.InfoFormat(CultureInfo.InvariantCulture, "Backing up config file before migration");
      var now = DateTime.Now;
      var backupFilePath = filePath + "_bck_" + now.ToString("yyyy-MM-dd") + "_" + now.Millisecond;
      File.Copy(filePath, backupFilePath);
      using (var file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
      {
        xmlDocument.Save(file);
      }
    }

    private void ApplySpecs(XmlDocument xmlDocument)
    {
      foreach (var spec in specs)
      {
        var specApplied = spec.Apply(xmlDocument);
        if (specApplied) 
        {
          dirty = true;
        }
      }
    }

    private interface ISpec
    {
      bool Apply(XmlDocument xmlDocument);
    }

    private class AddSpec : ISpec
    {
      public string CheckPath { get; private set; }
      public string LeftSibling { get; private set; }
      public string AddXmlElementFormat { get; private set; }
      public Func<XmlDocument, string>[] FuncArgs { get; private set; } 

      public AddSpec(string checkPath, string leftSibling, string addXmlElementFormat, params Func<XmlDocument, string>[] funcArgs)
      {
        CheckPath = checkPath;
        LeftSibling = leftSibling;
        AddXmlElementFormat = addXmlElementFormat;
        FuncArgs = funcArgs;
      }

      public bool Apply(XmlDocument xmlDocument)
      {
        var checkNode = xmlDocument.SelectSingleNode(CheckPath);
        if (checkNode != null) return false;

        var siblingNode = xmlDocument.SelectSingleNode(LeftSibling);
        if (siblingNode == null)
        {
          log.WarnFormat(CultureInfo.InvariantCulture, "Unable to migrate configuration. Path does not exist: {0}",
                         LeftSibling);
          return false;
        }

        var format = AddXmlElementFormat;
        var args = new string[FuncArgs.Length];
        for (var ix = 0; ix < FuncArgs.Length; ix++)
        {
          var s = FuncArgs[ix](xmlDocument);
          args[ix] = s;
          format = format.Replace("(arg" + ix + ")", "{" + ix + "}");
        }

        var newElement = xmlDocument.CreateDocumentFragment();
        newElement.InnerXml = string.Format(CultureInfo.InvariantCulture, format, args);

        siblingNode.ParentNode.InsertAfter(newElement, siblingNode);

        return true;
      }
    }

    private class RemoveSpec : ISpec
    {
      public string CheckPath { get; private set; }

      public RemoveSpec(string checkPath)
      {
        CheckPath = checkPath;
      }

      public bool Apply(XmlDocument xmlDocument)
      {
        var checkNode = xmlDocument.SelectSingleNode(CheckPath);
        if (checkNode == null) return false;

        checkNode.ParentNode.RemoveChild(checkNode);

        return true;
      }
    }

    private class ReplaceAttributeValueSpec : ISpec
    {
      public string AttributePath { get; private set; }
      public string ExistingValue { get; private set; }
      public string NewValue { get; private set; }

      public ReplaceAttributeValueSpec(string attributePath, string existingValue, string newValue)
      {
        AttributePath = attributePath;
        ExistingValue = existingValue;
        NewValue = newValue;
      }

      public bool Apply(XmlDocument xmlDocument)
      {
        var attributes = xmlDocument.SelectNodes(AttributePath);
        if (attributes.Count == 0) return false;

        var modified = false;
        foreach (XmlAttribute attribute in attributes)
        {
          if (!string.Equals(attribute.Value, ExistingValue, StringComparison.Ordinal))
          {
            continue;
          }
          modified = true;
          attribute.Value = NewValue;
        }

        return modified;
      }
    }


  }
}
