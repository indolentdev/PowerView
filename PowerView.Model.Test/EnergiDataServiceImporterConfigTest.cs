using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PowerView.Model.Test
{
    [TestFixture]
    public class EnergiDataServiceImporterConfigTest
    {
        [Test]
        public void ConstructorThrows()
        {
            // Arrange
            const bool importEnabled = true;
            var timeSpan = TimeSpan.FromDays(1);
            const string priceArea = "DK1";
            const string label = "main";
            const Unit currency = Unit.Dkk;

            // Act & Asssert
            Assert.That(() => new EnergiDataServiceImporterConfig(importEnabled, TimeSpan.Zero, priceArea, label, currency), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("timeSpan"));
            Assert.That(() => new EnergiDataServiceImporterConfig(importEnabled, TimeSpan.FromHours(25), priceArea, label, currency), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("timeSpan"));
            Assert.That(() => new EnergiDataServiceImporterConfig(importEnabled, timeSpan, null, label, currency), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("priceArea"));
            Assert.That(() => new EnergiDataServiceImporterConfig(importEnabled, timeSpan, string.Empty, label, currency), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("priceArea"));
            Assert.That(() => new EnergiDataServiceImporterConfig(importEnabled, timeSpan, priceArea, null, currency), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("label"));
            Assert.That(() => new EnergiDataServiceImporterConfig(importEnabled, timeSpan, priceArea, string.Empty, currency), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("label"));
            Assert.That(() => new EnergiDataServiceImporterConfig(importEnabled, timeSpan, priceArea, label, Unit.Watt), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("currency"));
            Assert.That(() => new EnergiDataServiceImporterConfig(importEnabled, timeSpan, priceArea, label, (Unit)89), Throws.TypeOf<DomainConstraintException>().And.Message.Contains("currency"));
        }

        [Test]
        public void ConstructorAndProperties()
        {
            // Arrange

            // Act
            var target = new EnergiDataServiceImporterConfig(true, TimeSpan.FromDays(1), "DK1", "main", Unit.Dkk);

            // Assert
            Assert.That(target.ImportEnabled, Is.EqualTo(true));
            Assert.That(target.TimeSpan, Is.EqualTo(TimeSpan.FromDays(1)));
            Assert.That(target.PriceArea, Is.EqualTo("DK1"));
            Assert.That(target.Label, Is.EqualTo("main"));
            Assert.That(target.Currency, Is.EqualTo(Unit.Dkk));
        }

        [Test]
        public void ConstructorSettingsThrows()
        {
            // Arrange
            var settings = new Dictionary<string, string>
            {
                { "EDSI_ImportEnabled", "true" },
                { "EDSI_TimeSpan", "1.00:00:00" },
                { "EDSI_PriceArea", "DK1" },
                { "EDSI_Label", "main" },
                { "EDSI_Currency", "Dkk" }
            };

            // Act & Assert
            Assert.That(() => new EnergiDataServiceImporterConfig(null), Throws.ArgumentNullException);
            Assert.That(() => new EnergiDataServiceImporterConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("EDSI_ImportEnabled", "true") }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("ImportEnabled"));
            Assert.That(() => new EnergiDataServiceImporterConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("EDSI_TimeSpan", "1.00:00:00") }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("TimeSpan"));
            Assert.That(() => new EnergiDataServiceImporterConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("EDSI_PriceArea", "DK1") }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("PriceArea"));
            Assert.That(() => new EnergiDataServiceImporterConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("EDSI_Label", "main") }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Label"));
            Assert.That(() => new EnergiDataServiceImporterConfig(
              settings.Except(new[] { new KeyValuePair<string, string>("EDSI_Currency", "Dkk") }).ToList()),
              Throws.TypeOf<DomainConstraintException>().And.Message.Contains("Currency"));
        }

        [Test]
        public void GetSettings()
        {
            // Arrange
            var target = new EnergiDataServiceImporterConfig(true, TimeSpan.FromDays(1), "DK1", "main", Unit.Dkk);

            // Act
            var settings = target.GetSettings();

            // Assert
            Assert.That(settings.Count, Is.EqualTo(5));
            Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("EDSI_ImportEnabled", "True")));
            Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("EDSI_TimeSpan", "1.00:00:00")));
            Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("EDSI_PriceArea", "DK1")));
            Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("EDSI_Label", "main")));
            Assert.That(settings, Contains.Item(new KeyValuePair<string, string>("EDSI_Currency", "Dkk")));
        }

        [Test]
        public void GetSettingsConstructorSettings()
        {
            // Arrange
            var target = new EnergiDataServiceImporterConfig(false, TimeSpan.FromHours(1), "DK2", "main", Unit.Eur);

            // Act
            var settings = target.GetSettings();
            var target2 = new EnergiDataServiceImporterConfig(settings);

            // Assert
            Assert.That(target2.ImportEnabled, Is.EqualTo(target.ImportEnabled));
            Assert.That(target2.TimeSpan, Is.EqualTo(target.TimeSpan));
            Assert.That(target2.PriceArea, Is.EqualTo(target.PriceArea));
            Assert.That(target2.Label, Is.EqualTo(target.Label));
            Assert.That(target2.Currency, Is.EqualTo(target.Currency));
        }

        [Test]
        public void ToStringTest()
        {
            // Arrange
            var target = new EnergiDataServiceImporterConfig(false, TimeSpan.FromHours(1), "DK2", "main", Unit.Eur);

            // Act
            var s = target.ToString();

            // Assert
            Assert.That(s, Contains.Substring("False"));
            Assert.That(s, Contains.Substring("01:00:00"));
            Assert.That(s, Contains.Substring("DK2"));
            Assert.That(s, Contains.Substring("main"));
            Assert.That(s, Contains.Substring("Eur"));
        }

    }
}
