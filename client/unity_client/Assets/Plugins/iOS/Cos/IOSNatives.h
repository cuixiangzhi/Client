//
//  IOSNatives.h
//  csvsdk
//
//  Created by Hydra on 2017/8/22.
//  Copyright © 2017年 Hydra. All rights reserved.
//

#import <Foundation/Foundation.h>


@interface IOSNatives : NSObject
+(void)SendMessageToUnityWithMessage:(const char*)msg toMethod:(const char*)methodName;
@end
