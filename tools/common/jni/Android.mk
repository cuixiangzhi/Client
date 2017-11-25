LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := common

LOCAL_CFLAGS := -fvisibility=hidden
LOCAL_LDLIBS := -landroid

LOCAL_C_INCLUDES := ..
LOCAL_SRC_FILES := \
$(wildcard ../client/*.cc) \
$(wildcard ../bsdiff/*.c) \
$(wildcard ../uuid/*.cpp)

#添加静态库
#生成动态库
include $(BUILD_SHARED_LIBRARY)
#导入静态库