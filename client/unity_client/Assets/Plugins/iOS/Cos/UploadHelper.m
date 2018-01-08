#import "UploadHelper.h"
#import "ISN_NativeCore.h"
#import <CommonCrypto/CommonDigest.h>
#import <CommonCrypto/CommonHMAC.h>
#import "COSClient.h"
#import "COSTask.h"
#import "IOSNatives.h"

static BOOL isNpLogDebug = false;

void NpLog(NSString* format, ...){
    if(isNpLogDebug){
        va_list args;
        va_start(args, format);
        NSLogv(format, args);
        va_end(args);
    }
}

@implementation UploadHelper
{
    NSString* _appId;
    NSString* _bucketName;
    NSString* _secretId;
    NSString* _secretKey;
    NSString* _region;
    long _currentTime;
    float _uploadProgress;
    float _downloadProgress;
    COSClient* _client;
}

+(id) instance{
    static UploadHelper* _instance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _instance = [[self alloc] init];
    });
    return _instance;
}

-(void)initCosWithAppId:(NSString*)appId withBucketName:(NSString*) bucketName withSecretId:(NSString*)secretId withSecretKey:(NSString*)secretKey withRegion:(NSString*) region isDebug:(BOOL) debug{
    _appId = appId;
    _bucketName = bucketName;
    _secretId = secretId;
    _secretKey = secretKey;
    _region = region;
    isNpLogDebug = debug;
    _client = [[COSClient alloc] initWithAppId:_appId withRegion:_region];
    [_client openHTTPSrequset:YES];
}

NSString * const UNITY_SPLITTER = @"|";
NSString * const UNITY_EOF = @"endofline";


-(NSString*)localSign:(NSString*)fileName isOnceSign:(BOOL) once{
    long current = _currentTime;
    long expried = current + 86400;
    long random = arc4random_uniform(100000);
    NSString* sign;
    if(once){
        expried = 0;
        NSString* fileId = [NSString stringWithFormat:@"/%@/%@/%@", _appId, _bucketName, [self urlEncode:fileName]  ];
        sign = [NSString stringWithFormat:@"a=%@&b=%@&k=%@&e=%ld&t=%ld&r=%ld&f=%@", _appId,
                _bucketName, _secretId, expried, current, random, fileId];
    }else{
        sign = [NSString stringWithFormat:@"a=%@&b=%@&k=%@&e=%ld&t=%ld&r=%ld&f=", _appId,
                _bucketName, _secretId, expried, current, random];
    }
    NSString* result =  [self hmac:sign withKey:_secretKey];
    NpLog(@"%@\n%@", sign, result);
    return result;
}

-(NSString*)urlEncode:(NSString*) value{
    return [value stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLHostAllowedCharacterSet]];
};


-(NSString *)hmac:(NSString *)plainText withKey:(NSString *)key
{
    const char *cKey  = [key cStringUsingEncoding:NSUTF8StringEncoding];
    const char *cData = [plainText cStringUsingEncoding:NSUTF8StringEncoding];
    
    unsigned char cHMAC[CC_SHA1_DIGEST_LENGTH];
    
    CCHmac(kCCHmacAlgSHA1, cKey, strlen(cKey), cData, strlen(cData), cHMAC);
    
    NSData *HMACData = [[NSData alloc] initWithBytes:cHMAC length:sizeof(cHMAC)];
    NSData *originByte = [plainText dataUsingEncoding:NSUTF8StringEncoding];
    NSMutableData *temp = [HMACData mutableCopy];
    [temp appendData:originByte];
    return [[temp copy] AsBase64String];
}

-(void)pushToCos:(NSString *)filePath withDir:(NSString *)dir currentTime:(long)time{

    _currentTime = time;
    COSObjectPutTask *task = [[COSObjectPutTask alloc] init];
    task.filePath = filePath;
    task.fileName = [filePath lastPathComponent];
    task.bucket = _bucketName;
    if(dir == nil) dir = @"";
    task.directory = dir;
    task.insertOnly = NO;
    task.sign = [self localSign:[filePath lastPathComponent] isOnceSign:NO];
    
    
    __weak UploadHelper* weakSelf = self;
    _client.completionHandler = ^(COSTaskRsp *resp, NSDictionary *context){
        COSObjectUploadTaskRsp *rsp = (COSObjectUploadTaskRsp *)resp;
        if (rsp.retCode == 0) {
            NSMutableString* resultString = [[NSMutableString alloc] init];
            [resultString appendString:@"上传结果：ret="];
            [resultString appendString:[@(rsp.retCode) stringValue]];
            [resultString appendString:@"; msg ="];
            [resultString appendString:rsp.descMsg];
            [resultString appendString:@"\n"];
            [resultString appendString:@" access_url= "];
            if (rsp.acessURL) {
                [resultString appendString:rsp.acessURL];
            }else{
                [resultString appendString:@"null"];
            }
            [resultString appendString:@"\n"];
            [resultString appendString:@" resource_path= "];
            if(rsp.sourceURL){
                [resultString appendString:rsp.sourceURL];
            }else{
                [resultString appendString:@"null"];
            }
            [resultString appendString:@"\n"];
            [resultString appendString:@" url= "];
            if(rsp.httpsURL){
                [resultString appendString:rsp.httpsURL];
            }else{
                [resultString appendString:@"null"];
            }
            NpLog(@"%@", resultString);
            
            
            [weakSelf sendRemoteFileInfoToUnity:0 withRemotePath:rsp.acessURL withLocalPath:filePath];
            
            NpLog(@"Upload Complete");
        }else{
            NpLog(@"%@\n%@",rsp.descMsg, rsp.description);
            [weakSelf sendRemoteFileInfoToUnity:-1 withRemotePath:[@(rsp.retCode)stringValue] withLocalPath:rsp.descMsg];
        }
        
    };
    __block int64_t total = 0;
    _client.progressHandler = ^(int64_t bytesWritten, int64_t totalBytesWritten, int64_t totalBytesExpectedToWrite) {
        NpLog(@"bytesWritten %lld totalBytesWritten %lld totalBytesExpectedToWrite %lld", bytesWritten, totalBytesWritten,
              totalBytesExpectedToWrite);
        total += totalBytesWritten;
        _uploadProgress =  total*1.0f/totalBytesExpectedToWrite;
    };
    _uploadProgress = 0;
    [_client putObject:task];
    
}

-(void)pushToCos:(NSString *)filePath currentTime:(long)time {
    [self pushToCos:filePath withDir:@"" currentTime:time];
}

-(void)getFromeCos:(NSString*)filePath saveTo:(NSString*) saveDir
 {
    COSObjectGetTask* task = [[COSObjectGetTask alloc] initWithUrl:filePath];
    
    __weak UploadHelper* weakSelf = self;
    _client.completionHandler = ^(COSTaskRsp *resp, NSDictionary *context){
        
        COSGetObjectTaskRsp *rsp = (COSGetObjectTaskRsp *)resp;
        if(rsp.retCode == 0){
            NSString* fileName = [filePath lastPathComponent];
            NSString* savePath = [NSString pathWithComponents:@[saveDir, fileName]];
            
            NSMutableString* resultString = [[NSMutableString alloc] init];
            [resultString appendString:@"下载结果：ret="];
            [resultString appendString:[@(rsp.retCode) stringValue]];
            [resultString appendString:@"; msg ="];
            [resultString appendString:rsp.descMsg];
            [resultString appendString:@"\n"];
            [resultString appendString:@" withObject= "];
            if (rsp.object) {
                [resultString appendString:@"YES"];
            }else{
                [resultString appendString:@"NO"];
            }
            [resultString appendString:@"\n"];
            [resultString appendString:@" savePath= "];
            [resultString appendString:savePath
];
            [resultString appendString:@"\n"];
            [resultString appendString:@" url= "];
            [resultString appendString:filePath];
        
            NpLog(@"%@", resultString);
            if(rsp.object){
                
                NSError * error = nil;
                [[NSFileManager defaultManager] createDirectoryAtPath:saveDir
                                          withIntermediateDirectories:YES
                                                           attributes:nil
                                                                error:&error];
                if (error != nil) {
                    NpLog(@"error creating directory: %@", error);
                    
                }
               
                [rsp.object writeToFile:savePath atomically:YES];
                [weakSelf sendDownloadFileInfoToUnity:0 withRemotePath:filePath withLocalPath:savePath
];
            }
        }else{
            NpLog(@"%@\n%@", rsp.descMsg, rsp.description);
            [weakSelf sendDownloadFileInfoToUnity:-1 withRemotePath:rsp.descMsg withLocalPath:rsp.description];
        }
    };
    __block int64_t total = 0;
    _client.downloadProgressHandler = ^(int64_t receiveLength,int64_t contentLength){
        NpLog(@"receiveLength %lld contentLength %lld", receiveLength, contentLength);
        total += receiveLength;
        _downloadProgress = total * 1.0f/contentLength;
    };
    _downloadProgress = 0;
    [_client getObject:task];
    
}

-(void)deleteFromCos:(NSString*)filePath currentTime:(long)time{
    _currentTime = time;
    NSArray* pathWords = [filePath componentsSeparatedByString:@"/"];
    NSMutableString *dir = [[NSMutableString alloc] init];
    for(int i = 0; i < pathWords.count - 1; ++i){
        [dir appendString:pathWords[i]];
        [dir appendString:@"/"];
    }
    NpLog(@"cosdir is:%@", dir);
    //删除需要向业务后台申请一次性签名
    COSObjectDeleteCommand *cm = [[COSObjectDeleteCommand alloc] initWithFile:[filePath lastPathComponent]
                                                                       bucket:_bucketName
                                                                    directory:dir
                                                                         sign:[self localSign:filePath isOnceSign:YES]];
    NpLog(@"---删除任务的-taskId---%lld",cm.taskId);
    
    __weak UploadHelper* weakSelf = self;
    
    _client.completionHandler = ^(COSTaskRsp *resp, NSDictionary *context){
        COSTaskRsp *rsp = (COSTaskRsp *)resp;
        if (rsp.retCode == 0) {
            NpLog(@"删除成功: ret=%d; msg=%@", rsp.retCode, rsp.descMsg);
            [weakSelf sendDeleteFileInfoToUnity:0 withCosCode:rsp.retCode withFilePath:filePath];
        }else
        {
            NpLog(@"删除失败: ret=%d; msg=%@", rsp.retCode, rsp.descMsg);
            [weakSelf sendDeleteFileInfoToUnity:-1 withCosCode:rsp.retCode withFilePath:rsp.descMsg];
        }
    };
    [_client deleteObject:cm];
}

-(void) sendRemoteFileInfoToUnity:(int)stateCode withRemotePath:(NSString*)remotePath withLocalPath:(NSString*) localPath{
    NSMutableString* resultString = [[NSMutableString alloc] init];
    [resultString appendString:[@(stateCode) stringValue]];
    [resultString appendString:UNITY_SPLITTER];
    [resultString appendString:remotePath];
    [resultString appendString:UNITY_SPLITTER];
    [resultString appendString:localPath];
    [IOSNatives SendMessageToUnityWithMessage:[ISN_DataConvertor NSStringToChar:resultString] toMethod:"OnPublish"];
    
}

-(void) sendDownloadFileInfoToUnity:(int)stateCode withRemotePath:(NSString*)remotePath withLocalPath:(NSString*) localPath{
    NSMutableString* resultString = [[NSMutableString alloc] init];
    [resultString appendString:[@(stateCode) stringValue]];
    [resultString appendString:UNITY_SPLITTER];
    [resultString appendString:remotePath];
    [resultString appendString:UNITY_SPLITTER];
    [resultString appendString:localPath];
    [IOSNatives SendMessageToUnityWithMessage:[ISN_DataConvertor NSStringToChar:resultString] toMethod:"OnDownload"];
}

-(void)sendDeleteFileInfoToUnity:(int)stateCode withCosCode:(int)cosCode withFilePath:(NSString*) filePath{
    NSMutableString* resultString = [[NSMutableString alloc] init];
    [resultString appendString:[@(stateCode) stringValue]];
    [resultString appendString:UNITY_SPLITTER];
    [resultString appendString:[@(cosCode) stringValue]];
    [resultString appendString:UNITY_SPLITTER];
    [resultString appendString:filePath];
    [IOSNatives SendMessageToUnityWithMessage:[ISN_DataConvertor NSStringToChar:resultString] toMethod:"OnDeleteFile"];
}

-(float)getUploadProgress{
    return _uploadProgress;
}

-(float)getDownlaodProgress{
    return _downloadProgress;
}

@end
