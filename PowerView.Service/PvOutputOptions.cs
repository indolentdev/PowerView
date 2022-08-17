using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;


namespace PowerView.Service
{
    public class PvOutputOptions : IOptions<PvOutputOptions>
    {
        [Required(AllowEmptyStrings = false)]
        public Uri PvOutputAddStatusUrl { get; set; } = new Uri("http://pvoutput.org/service/r2/addstatus.jsp");

        [MinLength(1)]
        public string PvDeviceLabel { get; set; }

        [MinLength(1)]
        public string PvDeviceId { get; set;}

        [MinLength(1)]
        public string PvDeviceIdParam { get; set; } = "v12";

        [MinLength(1)]
        public string ActualPowerP23L1Param { get; set; } = "v7";

        [MinLength(1)]
        public string ActualPowerP23L2Param { get; set; } = "v8";

        [MinLength(1)]
        public string ActualPowerP23L3Param { get; set; } = "v9";

        PvOutputOptions IOptions<PvOutputOptions>.Value => this;
    }

}
