export let exportGaugesQuarterly = {
   "timestamps": [ 
      "2017-09-18T04:00:00.0000000Z", 
      "2017-09-18T04:15:00.0000000Z", 
      "2017-09-18T04:30:00.0000000Z", 
   ],
   "series": [
      {
         "label": "Water",
         "obisCode": "8.0.1.0.0.255",
         "values": [
            {
               "timestamp": "2017-09-18T04:01:00.0000000Z",
               "value": 12345.6,
               "unit": "m3",
               "deviceId": "SN1"
            },
            {
               "timestamp": "2017-09-18T04:17:00.0000000Z",
               "value": 12345.7,
               "unit": "m3",
               "deviceId": "SN1"
            },
            {
               "timestamp": "2017-09-18T04:33:00.0000000Z",
               "value": 4321,
               "unit": "m3",
               "deviceId": "SN3"
            }
         ]
      },
      {
         "label": "Main",
         "obisCode": "1.0.1.8.0.255",
         "values": [
            {
               "timestamp": "2017-09-18T04:03:00.0000000Z",
               "value": 456789,
               "unit": "WattHour",
               "deviceId": "SN2"
            },
            {
               "timestamp": null,
               "value": null,
               "diffValue": null,
               "unit": null,
               "deviceId": null
            },
            {
               "timestamp": "2017-09-18T04:31:00.0000000Z",
               "value": 456889,
               "unit": "WattHour",
               "deviceId": "SN2"
            }
         ]
      }
   ]
}
;