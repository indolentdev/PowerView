using System;
using System.Collections.Generic;
using System.Data;

namespace PowerView.Model.Repository
{
    public interface IImportRepository
    {
        ICollection<Import> GetImports();

        void AddImport(Import import);

        void DeleteImport(string label);

        void SetEnabled(string label, bool enabled);

        void SetCurrentTimestamp(string label, DateTime currentTimestamp);
    }
}
