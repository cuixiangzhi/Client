LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := common

LOCAL_CPP_EXTENSION := .cc .c .cpp
#LOCAL_CFLAGS := -D
#LOCAL_CPPFLAGS := $(LOCAL_CFLAGS)
#LOCAL_LDLIBS := 

LOCAL_C_INCLUDES :=

LOCAL_SRC_FILES :=

#$(call import-add-path $(LOCAL_PATH))
#LOCAL_WHOLE_STATIC_LIBRARIES := 

include $(BUILD_SHARED_LIBRARY)

#$(call import-add-module,)