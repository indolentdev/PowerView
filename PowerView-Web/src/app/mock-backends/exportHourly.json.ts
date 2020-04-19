export let exportHourly = {
   "timestamps": [ 
      "2017-09-18T04:00:00.0000000Z", 
      "2017-09-18T05:00:00.0000000Z" 
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
               "serialNumber": "SN1"
            },
            {
               "timestamp": "2017-09-18T05:02:00.0000000Z",
               "value": 12345.7,
               "diffValue": 0.1,
               "unit": "m3",
               "serialNumber": "SN1"
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
               "serialNumber": "SN2"
            },
            {
               "timestamp": "2017-09-18T05:01:00.0000000Z",
               "value": 456889,
               "diffValue": 100,
               "unit": "WattHour",
               "serialNumber": "SN2"
            }
         ]
      }
   ]
}
;