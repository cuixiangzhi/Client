//
//  IOSNative.m
//  csvsdk
//
//  Created by Hydra on 2017/7/9.
//  Copyright © 2017年 Hydra. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "UploadHelper.h"
#import "ISN_NativeCore.h"
#import "IOSNatives.h"

extern void UnitySendMessage(const char* obj, const char* method, const char* msg);
@implementation IOSNatives
+(void)SendMessageToUnityWithMessage:(const char*)msg toMethod:(const char*)methodName{
    UnitySendMessage("TXCosManager", methodName, msg);
}


@end

#ifdef cplusplus
extern "C" {
#endif
    
    void _ISN_INITCOS(char* appId, char* bucketName, char* secretId, char* secretKey, char* region, bool isDebug){
        [[UploadHelper instance] initCosWithAppId:[ISN_DataConvertor charToNSString:appId] withBucketName:[ISN_DataConvertor charToNSString:bucketName] withSecretId:[ISN_DataConvertor charToNSString:secretId] withSecretKey:[ISN_DataConvertor charToNSString:secretKey] withRegion:[ISN_DataConvertor charToNSString:region] isDebug:isDebug];
    }
    
    void _ISN_TakeShortVideo(){
    }
    
    void _ISN_PlayShortVideo(char* path){
    }
    
    void _ISN_PlayLocalShortVideo(char* path){
    }
    
    

    void _ISN_UploadFile(char* path, char* dir, long time){
        [[UploadHelper instance] pushToCos:[ISN_DataConvertor charToNSString:path] withDir:[ISN_DataConvertor charToNSString:dir] currentTime:time];
    }
    
    void _ISN_DownloadFile(char* url, char* savePath, long time){
        [[UploadHelper instance] getFromeCos:[ISN_DataConvertor charToNSString:url] saveTo:[ISN_DataConvertor charToNSString:savePath]];
    }
    
    float _ISN_GetUploadProgress(){return [[UploadHelper instance] getUploadProgress];}
    float _ISN_GetDownloadProgress(){return [[UploadHelper instance] getDownloadProgress];}
    void _ISN_DeleteFileFromCos(char* filePath, long time){
        [[UploadHelper instance] deleteFromCos:[ISN_DataConvertor charToNSString:filePath] currentTime:time];
    }
#ifdef cplusplus
}
#endif
