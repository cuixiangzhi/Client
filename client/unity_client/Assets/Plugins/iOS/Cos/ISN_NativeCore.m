//  ISN_NativeCore.m
//  Unity-iPhone
//
//  Created by lacost on 9/6/15.
//
//

#import <Foundation/Foundation.h>

#import "ISN_NativeCore.h"



NSString * const ISN_NativeCore_UNITY_SPLITTER = @"|";
NSString * const ISN_NativeCore_UNITY_EOF = @"endofline";
NSString * const ISN_NativeCore_ARRAY_SPLITTER = @"%%%";


@implementation NSData (Base64)

+ (NSData *)InitFromBase64String:(NSString *)aString {
    return [[NSData alloc] initWithBase64Encoding:aString];
}

- (NSString *)AsBase64String {
    return [self base64EncodedStringWithOptions:0];
}

@end


@implementation NSDictionary (JSON)

- (NSString *)AsJSONString {
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:self options:0  error:&error];
    
    if (!jsonData) {
        return @"{}";
    } else {
        return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
}

@end


@implementation ISN_DataConvertor


+(NSString *) charToNSString:(char *)value {
    if (value != NULL) {
        return [NSString stringWithUTF8String: value];
    } else {
        return [NSString stringWithUTF8String: ""];
    }
}

+(const char *)NSIntToChar:(NSInteger)value {
    NSString *tmp = [NSString stringWithFormat:@"%ld", (long)value];
    return [tmp UTF8String];
}

+ (const char *) NSStringToChar:(NSString *)value {
    return [value UTF8String];
}

+ (NSArray *)charToNSArray:(char *)value {
    NSString* strValue = [ISN_DataConvertor charToNSString:value];
    
    NSArray *array;
    if([strValue length] == 0) {
        array = [[NSArray alloc] init];
    } else {
        array = [strValue componentsSeparatedByString:ISN_NativeCore_ARRAY_SPLITTER];
    }
    
    return array;
}

+ (const char *) NSStringsArrayToChar:(NSArray *) array {
    return [ISN_DataConvertor NSStringToChar:[ISN_DataConvertor serializeNSStringsArray:array]];
}

+ (NSString *) serializeNSStringsArray:(NSArray *) array {
    
    NSMutableString * data = [[NSMutableString alloc] init];
    
    
    for(NSString* str in array) {
        [data appendString:str];
        [data appendString: ISN_NativeCore_ARRAY_SPLITTER];
    }
    
    [data appendString: ISN_NativeCore_UNITY_EOF];
    
    NSString *str = [data copy];
    
    return str;
}


+ (NSString *)serializeErrorToNSString:(NSError *)error {
    NSString* description = @"";
    if(error.description != nil) {
        description = error.description;
    }
    
    return  [self serializeErrorWithDataToNSString:description code: (int) error.code];
}

+ (NSString *)serializeErrorWithDataToNSString:(NSString *)description code:(int)code {
    NSMutableString * data = [[NSMutableString alloc] init];
    
    
    [data appendString: [NSString stringWithFormat:@"%d", code]];
    [data appendString: ISN_NativeCore_UNITY_SPLITTER];
    [data appendString: description];
    
    
    NSString *str = [data copy];
    
    return  str;
}



+ (const char *) serializeErrorWithData:(NSString *)description code: (int) code {
    NSString *str = [ISN_DataConvertor serializeErrorWithDataToNSString:description code:code];
    return [ISN_DataConvertor NSStringToChar:str];
}

+ (const char *) serializeError:(NSError *)error  {
    NSString *str = [ISN_DataConvertor serializeErrorToNSString:error];
    return [ISN_DataConvertor NSStringToChar:str];
}

@end
