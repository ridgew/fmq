db.getCollection('BusInvokeLog').count();
db.getCollection('BusInvokeLog').find({}).limit(50).sort({ Invoked: -1 });

new Date("2017-03-16T07:22:00Z")

db.getCollection('OperationLog').find({}).limit(50).sort({ OpTime: -1 });


//2.1比较  > $gt , >= $gte, < $lt, <= $lte, != $ne  $mod
//2.2 $in & $nin
// -- ...find({age:{$mod:[11,0]}})
//------------
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-04-08T07:22:00Z") }, 

       "ExMsg": "IDH.Service.Bus.Email.EmailService::SynchronousAccountMessage" }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }

    )

    .limit(200).sort({"ExTime": -1, "ExDetail":-1 })


// AmazonOrderFulfillmentMarkedEvent
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-07-08T07:22:00Z") }, 
       "Action": { $regex: /AmazonOrderFulfillmentMarkedEvent/ }
        
    }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(200).sort({"ExTime": -1, "ExDetail":-1 })


//WishProductListingCollection
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-09-03T07:22:00Z") }, 
       "Action": { $regex: /WishProductListingCollection/ }
    }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(200).sort({"ExTime": -1, "ExDetail":-1 })
    
//ValidationWishProductIdCommand
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-08-03T07:22:00Z") }, 
       "Action": { $regex: /ValidationWishProductIdCommand/ }
    }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(200).sort({"ExTime": -1, "ExDetail":-1 }) 
    


db.getCollection('BusInvokeLog').find({'Action':'IDH.Service.Bus.AmazonSalesSyn.GetReportRequestListEvent'
    ,'Invoked':{$gt:new Date("2017-06-026T22:00:00Z"), $lt:new Date("2017-06-26T22:30:00Z")}})
    .limit(200).sort({'Invoked':-1})
    //.select({'Invoked':1,'CorrelationId':1})


db.getCollection('BusInvokeLog').find({'CorrelationId':'21a09fa3-a8e8-45f0-abd3-f5a42ae5c679'})

db.getCollection('BusInvokeLog').find({'Action':'IDH.Service.Bus.AmazonSalesSyn.GetReportRequestListEvent'
    ,'Invoked':{$gt:new Date("2017-06-026T22:00:00Z"), $lt:new Date("2017-06-26T22:30:00Z")}})
    .count();


//查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-09-26T05:55:00Z") }, "Action": { $regex:/Schedule/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(2000).sort({ ExTime: -1 })

db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-09-26T05:55:00Z") }, "ExMsg": { $regex:/Schedule/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(2000).sort({ ExTime: -1 })
    
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-09-26T05:55:00Z") }, "ExMsg": { $regex:/Amazon/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(2000).sort({ ExTime: -1 })

db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-09-26T05:55:00Z") }, "Severity": { $lte: 3 }, "ExDetail": { $regex:/Amazon/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(2000).sort({ ExTime: -1 })


db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-09-26T05:55:00Z") }, "Severity": { $lte: 3 }, "ExDetail": { $regex:/Wish/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(2000).sort({ ExTime: -1 })
    

//查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-10-15T05:55:00Z") }, "SvrIP": { $regex:/\./ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(2000).sort({ ExTime: -1 })
    
    
//查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-07-28T07:22:00Z") }, "SvrIP": { $regex:/192.168.1.242/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })
    
    
db.getCollection('ExceptionLog')
    .find({ "ExMsg": { $regex: /ShpostWishLib.ShpostWishApi::Log/ }, "ExTime": { $gte: new Date("2017-07-28T07:22:00Z") } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })
    

db.ExceptionLog.find({'Action':'IDH.Service.Bus.WishSalesSyn.WishVariantInventorySettingCommand',
    'ExTime':{$gt:new Date("2017.8.23")}}).limit(20).sort({'ExTime':-1})



 //查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-08-08T07:22:00Z") }, "Action": { $regex: /ListingReplenishCommand/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(2000).sort({ ExTime: -1 })
    


 //查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-08-08T07:22:00Z") }, "Action": { $regex: /ActiveListingQuotingReceived/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })
    


 //查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-09-28T07:22:00Z") }, "Action": { $regex: /ValidationWishProductIdCommand/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })
    

 //查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-08-08T07:22:00Z") }, "Action": { $regex: /WishVariantInventorySettingCommand/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })
    
    
 //查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-08-28T07:22:00Z") }, "Action": { $regex: /SkuStorageClearanceEvent/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })
    

 //查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-07-08T07:22:00Z") }, "Action": { $regex: /ItemCategoryChanged/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })
 
    
/* */
db.getCollection('ExceptionLog')

    .find({ "ExTime": { $gte: new Date("2017-08-08T07:22:00Z") },"SvrIP":"192.168.1.12" }

    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1,"ExDetail": 1 }

    )

    .limit(200).sort({ ExTime: -1 })

    
    


db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-04-08T07:22:00Z") }, "ExTime": { $lte: new Date("2017-04-10T07:22:00Z") } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })



//查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-07-28T07:22:00Z") }, "Action": { $regex: /PublishItemToWishCommand/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(500).sort({ ExTime: -1 })
    

//查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-03-28T07:22:00Z") }, "Action": "PublishListingOnline" }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })
    

//查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-07-28T07:22:00Z") },"Action": { $regex: /SyncWishDataToListingCommand/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })


//查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-07-16T07:22:00Z") }, "Severity": { $lte: 5 }, "SvrIP":{ $regex: /192\.168\.1\.242/ } }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })


//查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-03-16T07:22:00Z") }, "SvrIP": "192.168.1.12" }
    , { "ExTime": 1, _id: 0, "SvrIP": 1, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })

//查询API请求异常记录
db.getCollection('ExceptionLog')
    .find({ "ExTime": { $gte: new Date("2017-09-30 12:04:52") }, "ExMsg": "WishListingCreatedEvent" }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })






//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ThrottingHandler/ },
        "ExTime": { $gte: new Date("2017-07-27T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })

//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ThrottingHandler/ },"ExMsg": { $regex: /throttled/ },
        "ExTime": { $gte: new Date("2017-07-27T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })



//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ThrottingHandler::ProductMatchedHandler/ },
        "ExTime": { $gte: new Date("2017-06-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })
    
    

//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /IDH.Service.Customer.CustomerService::ParseEmailRawMessage/ },
        "ExTime": { $gte: new Date("2017-05-01T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })


//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ThrottingHandler::GetMyPriceForSkuHandler/ },
        "ExTime": { $gte: new Date("2017-03-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })



//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ThrottingHandler::SubmissionFeedHandler/ },
        "ExTime": { $gte: new Date("2017-07-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })


//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /GetItemSaleStatusCommand/ },
        "ExTime": { $gte: new Date("2017-06-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })  


//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /GetWishSubListingSoldItemCommand/ },
        "ExTime": { $gte: new Date("2017-09-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })  
    


//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /GetAmazonSoldQuotingItemCommmand/ },
        "ExTime": { $gte: new Date("2017-08-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })  



//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ItemCreateListingCommand/ },
        "ExTime": { $gte: new Date("2017-09-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 }) 



//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ItemAutoCreateListingCommand/ },
        "ExTime": { $gte: new Date("2017-09-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 }) 
    

//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ItemAutoCreateWishListingCommand/ },
        "ExTime": { $gte: new Date("2017-09-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 }) 
    
    

//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /AmazonSalesSyn.InventoryReportReceived/ },
        "ExTime": { $gte: new Date("2017-08-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })  
    

//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "ExDetail": { $regex: /is\sout\sof\srange/ },
        "ExTime": { $gte: new Date("2017-07-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })  


 
//查询API请求异常记录 Action
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ThrottingHandler::GetSubmissionFeedResultHandler/ },
        "ExTime": { $gte: new Date("2017-06-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })  
    


db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /IDH.Service.Bus.Email.EmailService::SynchronousAccountMessage/ },
        "ExTime": { $gte: new Date("2017-03-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })




db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /IDH.Service.Customer.CustomerService::ParseEmailRawMessage/ },
        "ExTime": { $gte: new Date("2017-03-20T07:22:00Z") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })




//Action like amazon
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /amazon/ig },
        "ExTime": { $gte: new Date("2017-03-09 12:04:52") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })


//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//查询API请求异常记录 ReqMsg
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ThrottingHandler/ }, "ReqMsg": { $regex: /(A1KSPNM5AQYQNH)/ },
        "ExTime": { $gte: new Date("2017-07-03 12:04:52") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })



//查询API请求异常记录 ReqMsg
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ThrottingHandler/ }, "ReqMsg": { $regex: /(A1AM78C64UM0Y8)|(ATVPDKIKX0DER)/ },
        "ExTime": { $gte: new Date("2017-03-08 12:04:52") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })


//查询API请求异常记录 ReqMsg
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ThrottingHandler::RequestReportHandler/ }, "ReqMsg": { $regex: /(A1AM78C64UM0Y8)|(ATVPDKIKX0DER)/ },
        "ExTime": { $gte: new Date("2017-03-08 12:04:52") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })


//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


//查询API请求异常记录 ReqMsg
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /ThrottingHandler/ }, "ExMsg": { $regex: /asin/ },
        "ExTime": { $gte: new Date("2017-03-05 12:04:52") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })


//查询请求异常记录总数
var q = db.getCollection('ExceptionLog')
    .find({ "Action": { $regex: /IDH\.Service\.Bus\.Shipping\.RegistryConfirmation/ } }
    );
q.count();


//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


//请求异常记录
db.getCollection('ExceptionLog')
    .find({
        "ExTime": { $gte: new Date("2017-07-09 12:04:52") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })


//查询API请求异常记录 Wish
db.getCollection('ExceptionLog')
    .find({
        "Action": { $regex: /Wish/ },"ExTime": { $gte: new Date("2017-06-05 12:04:52") }
    }
    , { "ExTime": 1, _id: 0, "Contract": 1, "Action": 1, "Severity": 1, "ReqMsg": 1, "ExMsg": 1, "ExDetail": 1 }
    )
    .limit(200).sort({ ExTime: -1 })


/*#########################################
》》可能会用到的Log查询（以表OperationLog为例子，mongo db称表为collections??
#########################################*/
//按日期倒叙查询最新的10条操作日??{-1:desc,  1:asc}
db.getCollection('OperationLog').find({}).sort({ OpTime: -1 }).limit(10)

//查找Action=GetUriNotesList的最??0条操作日志，按OpTime DESC排列
db.getCollection('OperationLog').find({ "Action": "GetUriNotesList" }).sort({ OpTime: -1 }).limit(10)

//查询Contract=IProductService，Action=GetNewProductList，OpTime>=2017-03-08 12:04:52的操作日志并按时间DESC排列
db.getCollection('OperationLog').find({
    "Contract": "IProductService",
    "Action": "GetNewProductList",
    "OpTime": { $gte: new Date("2017-03-08 12:04:52") }
}).sort({ OpTime: -1 })

//in 语句
db.getCollection('OperationLog').find({
    "Action": { $in: ["GetNewProductList", "GetUriNotesList"] },
    "OpTime": { $gte: new Date("2017-03-07 12:04:52") }
}).sort({ OpTime: -1 })

db.getCollection('OperationLog').find({
    "OpTime": { $gte: new Date("2017-03-07 12:04:52") }
}).sort({ OpTime: -1 })
