LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := uuid

LOCAL_CPP_EXTENSION := .cpp

LOCAL_C_INCLUDES := \
$(LOCAL_PATH)/../uuid

LOCAL_SRC_FILES := \
$(LOCAL_PATH)/bitcoder.cpp
$(LOCAL_PATH)/md5.cpp
$(LOCAL_PATH)/uuid32.cpp

include $(BUILD_STATIC_LIBRARY)