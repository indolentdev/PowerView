using System.Collections.Generic;

namespace PowerView.Model.Repository;

public interface ISettingRepository
{
    void Upsert(string name, string value);

    void Upsert(ICollection<KeyValuePair<string, string>> items);

    string Get(string name);

    IList<KeyValuePair<string, string>> Find(string startsWith);

    MqttConfig GetMqttConfig();

    void UpsertMqttConfig(MqttConfig mqttConfig);

    SmtpConfig GetSmtpConfig();

    void UpsertSmtpConfig(SmtpConfig smtpConfig);

    EnergiDataServiceImportConfig GetEnergiDataServiceImportConfig();

    void UpsertEnergiDataServiceImportConfig(EnergiDataServiceImportConfig edsiConfig);

    DateTime? GetEnergiDataServiceImportPosition();

    void UpsertEnergiDataServiceImportPosition(DateTime position);

}
