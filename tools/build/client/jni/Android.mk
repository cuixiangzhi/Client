LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := gameutil

LOCAL_CFLAGS := -fvisibility=hidden
LOCAL_LDLIBS := -landroid

LOCAL_C_INCLUDES := ./..
LOCAL_C_INCLUDES += ./../../..
LOCAL_SRC_FILES := $(wildcard ./../*.cc)
LOCAL_SRC_FILES += $(wildcard ./../../../bsdiff/*.c)
LOCAL_SRC_FILES += $(wildcard ./../../../uuid/*.cpp)

include $(BUILD_SHARED_LIBRARY)
