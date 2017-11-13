LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := network

LOCAL_CPP_EXTENSION := .cc

LOCAL_C_INCLUDES := \
$(LOCAL_PATH)/../network

LOCAL_SRC_FILES := \
$(LOCAL_PATH)/socket.cc

include $(BUILD_STATIC_LIBRARY)