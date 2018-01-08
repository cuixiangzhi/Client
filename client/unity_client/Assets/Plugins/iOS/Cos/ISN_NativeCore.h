//
//  ISN_NativeCore.h
//  Unity-iPhone
//
//  Created by lacost on 9/6/15.
//
//


@interface NSData (Base64)

+ (NSData *)InitFromBase64String:(NSString *)aString;
- (NSString *)AsBase64String;

@end

@interface NSDictionary (JSON)

- (NSString *)AsJSONString;

@end

@interface ISN_DataConvertor : NSObject

+ (NSString*) charToNSString: (char*)value;
+ (const char *) NSIntToChar: (NSInteger) value;
+ (const char *) NSStringToChar: (NSString *) value;
+ (NSArray*) charToNSArray: (char*)value;

+ (const char *) serializeErrorWithData:(NSString *)description code: (int) code;
+ (const char *) serializeError:(NSError *)error;

+ (NSString *) serializeErrorWithDataToNSString:(NSString *)description code: (int) code;
+ (NSString *) serializeErrorToNSString:(NSError *)error;


+ (const char *) NSStringsArrayToChar:(NSArray *) array;
+ (NSString *) serializeNSStringsArray:(NSArray *) array;

@end
