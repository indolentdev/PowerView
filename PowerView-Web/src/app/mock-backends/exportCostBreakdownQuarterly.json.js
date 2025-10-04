export let exportCostBreakdownQuarterly = {
   "title": "El Subscription",
   "currency": "DKK",
   "vat": 25,
   "periods": [ 
      { "from":"2017-09-18T04:00:00.0000000Z", "to":"2017-09-18T04:15:00.0000000Z" },
      { "from":"2017-09-18T04:15:00.0000000Z", "to":"2017-09-18T04:30:00.0000000Z" },
      { "from":"2017-09-18T04:30:00.0000000Z", "to":"2017-09-18T04:45:00.0000000Z" }
   ],
   "entries": [
      {
         "name": "Tariff C",
         "values": [
            {
               "from": "2017-09-18T04:00:00.0000000Z",
               "to": "2017-09-18T04:15:00.0000000Z",
               "value": 23,
            },
            {
               "from": "2017-09-18T04:15:00.0000000Z",
               "to": "2017-09-18T04:30:00.0000000Z",
               "value": 0.1,
            },
            {
               "from": null,
               "to": null,
               "value": null,
            }
         ]
      },
      {
         "name": "Contribution",
         "values": [
            {
               "from": "2017-09-18T04:00:00.0000000Z",
               "to": "2017-09-18T04:15:00.0000000Z",
               "value": 12,
            },
            {
               "from": null,
               "to": null,
               "value": null,
            },
            {
               "from": "2017-09-18T04:30:00.0000000Z",
               "to": "2017-09-18T04:45:00.0000000Z",
               "value": 4,
            }
         ]
      }
   ]
}
;