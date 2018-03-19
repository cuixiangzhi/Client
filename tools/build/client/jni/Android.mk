LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := common

LOCAL_CFLAGS := -fvisibility=hidden
LOCAL_LDLIBS := -landroid

LOCAL_C_INCLUDES := ./..
LOCAL_SRC_FILES := $(wildcard ./../*.cc)
LOCAL_SRC_FILES += $(wildcard ./../../../bsdiff/*.c)
LOCAL_SRC_FILES += $(wildcard ./../../../uuid/*.cpp)
LOCAL_SRC_FILES += $(wildcard ./../../../io/*.cc)

include $(BUILD_SHARED_LIBRARY)
