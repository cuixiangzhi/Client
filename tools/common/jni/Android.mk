LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := common

LOCAL_CFLAGS := -frtti -DHAVE_PTHREAD -fvisibility=hidden

LOCAL_C_INCLUDES := ..
LOCAL_SRC_FILES := \
$(wildcard ../*.cc) \
$(wildcard ../bsdiff/*.c) \
$(wildcard ../uuid/*.cpp) \
$(wildcard ../network/*.cc) \
$(wildcard ../google/protobuf/*.cc) \
$(wildcard ../google/protobuf/io/*.cc) \
$(wildcard ../google/protobuf/compiler/*.cc) \
$(wildcard ../google/protobuf/stubs/*.cc) \
$(wildcard ../google/protobuf/util/*.cc) \
$(wildcard ../google/protobuf/util/internal/*.cc)

#添加静态库
#生成动态库
include $(BUILD_SHARED_LIBRARY)
#导入静态库