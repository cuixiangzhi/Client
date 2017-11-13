LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := common

#LOCAL_C_FLAGS := -lstdc++

LOCAL_C_INCLUDES := \
../ \
../bsdiff/ \
../uuid/ \
../network/ \

LOCAL_SRC_FILES := \
$(wildcard ../*.cc) \
$(wildcard ../bsdiff/*.c) \
$(wildcard ../uuid/*.cpp) \
$(wildcard ../network/*.cc) \
$(wildcard ../google/protobuf/*.cc) \
$(wildcard ../google/protobuf/io/*.cc) \
$(wildcard ../google/protobuf/compiler/*.cc) \
$(wildcard ../google/protobuf/stubs/*.cc) \
$(wildcard ../google/protobuf/util/*.cc)

#��Ӿ�̬��
#���ɶ�̬��
include $(BUILD_SHARED_LIBRARY)
#���뾲̬��