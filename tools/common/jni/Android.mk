LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := common

LOCAL_CPP_EXTENSION := .cc .c .cpp

LOCAL_C_INCLUDES :=
LOCAL_SRC_FILES :=

#添加静态库
$(call import-add-path $(LOCAL_PATH)/..)
LOCAL_WHOLE_STATIC_LIBRARIES := protobuf
LOCAL_WHOLE_STATIC_LIBRARIES += bsdiff
LOCAL_WHOLE_STATIC_LIBRARIES += network
LOCAL_WHOLE_STATIC_LIBRARIES += uuid
#生成动态库
include $(BUILD_SHARED_LIBRARY)
#导入静态库
$(call import-add-module,../google)
$(call import-add-module,../bsdiff)
$(call import-add-module,../network)
$(call import-add-module,../uuid)