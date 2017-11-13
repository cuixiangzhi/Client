LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := bsdiff

LOCAL_CPP_EXTENSION := .c

LOCAL_C_INCLUDES := \
$(LOCAL_PATH)/../bsdiff

LOCAL_SRC_FILES := \
$(LOCAL_PATH)/blocksort.c
$(LOCAL_PATH)/bsdiff.c
$(LOCAL_PATH)/bspatch.c
$(LOCAL_PATH)/bzlib.c
$(LOCAL_PATH)/compress.c
$(LOCAL_PATH)/crctable.c
$(LOCAL_PATH)/decompress.c
$(LOCAL_PATH)/huffman.c
$(LOCAL_PATH)/randtable.c

include $(BUILD_STATIC_LIBRARY)