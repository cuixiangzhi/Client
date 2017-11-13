LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := common

LOCAL_CPP_EXTENSION := .cc .c .cpp

LOCAL_C_INCLUDES := \
../
../bsdiff/ \
../uuid/ \
../network/ \
../google/protobuf/ \
../google/protobuf/io/ \
../google/protobuf/compiler/ \
../google/protobuf/stubs/ \
../google/protobuf/util/

LOCAL_SRC_FILES := \
../*.c \
../*.cc \
../*.cpp

#添加静态库
#生成动态库
include $(BUILD_SHARED_LIBRARY)
#导入静态库