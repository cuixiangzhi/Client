#ifndef UploadHelper_h
#define UploadHelper_h
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface UploadHelper : NSObject

+(id)instance;
-(void)pushToCos:(NSString *)filePath currentTime:(long)time;
-(void)pushToCos:(NSString *)filePath withDir:(NSString *)dir currentTime:(long)time;
-(void)getFromeCos:(NSString*)filePath saveTo:(NSString*) savePath;
-(float)getUploadProgress;
-(float)getDownloadProgress;
-(void)initCosWithAppId:(NSString*)appId withBucketName:(NSString*) bucketName withSecretId:(NSString*)secretId withSecretKey:(NSString*)secretKey withRegion:(NSString*) region isDebug:(BOOL) debug;
-(void)deleteFromCos:(NSString*)filePath currentTime:(long)time;
@end

#endif /* UploadHelper_h */
