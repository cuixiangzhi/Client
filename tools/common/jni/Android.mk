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

#��Ӿ�̬��
#���ɶ�̬��
include $(BUILD_SHARED_LIBRARY)
#���뾲̬��