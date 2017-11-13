LOCAL_PATH := $(call my-dir)
include $(CLEAR_VARS)
LOCAL_MODULE := protobuf

LOCAL_CPP_EXTENSION := .cc

LOCAL_C_INCLUDES := \
$(LOCAL_PATH)/../google/protobuf \
$(LOCAL_PATH)/../google/protobuf/io \
$(LOCAL_PATH)/../google/protobuf/compiler \
$(LOCAL_PATH)/../google/protobuf/stubs \
$(LOCAL_PATH)/../google/protobuf/util \

LOCAL_SRC_FILES := \
$(LOCAL_PATH)/../google/protobuf/any.cc \
$(LOCAL_PATH)/../google/protobuf/any.pb.cc \
$(LOCAL_PATH)/../google/protobuf/arena.cc \
$(LOCAL_PATH)/../google/protobuf/arenastring.cc \
$(LOCAL_PATH)/../google/protobuf/compiler/importer.cc \
$(LOCAL_PATH)/../google/protobuf/compiler/parser.cc \
$(LOCAL_PATH)/../google/protobuf/descriptor.cc \
$(LOCAL_PATH)/../google/protobuf/descriptor.pb.cc \
$(LOCAL_PATH)/../google/protobuf/descriptor_database.cc \
$(LOCAL_PATH)/../google/protobuf/duration.pb.cc \
$(LOCAL_PATH)/../google/protobuf/dynamic_message.cc \
$(LOCAL_PATH)/../google/protobuf/extension_set.cc \
$(LOCAL_PATH)/../google/protobuf/extension_set_heavy.cc \
$(LOCAL_PATH)/../google/protobuf/field_mask.pb.cc \
$(LOCAL_PATH)/../google/protobuf/generated_message_reflection.cc \
$(LOCAL_PATH)/../google/protobuf/generated_message_util.cc \
$(LOCAL_PATH)/../google/protobuf/io/coded_stream.cc \
$(LOCAL_PATH)/../google/protobuf/io/gzip_stream.cc \
$(LOCAL_PATH)/../google/protobuf/io/printer.cc \
$(LOCAL_PATH)/../google/protobuf/io/strtod.cc \
$(LOCAL_PATH)/../google/protobuf/io/tokenizer.cc \
$(LOCAL_PATH)/../google/protobuf/io/zero_copy_stream.cc \
$(LOCAL_PATH)/../google/protobuf/io/zero_copy_stream_impl.cc \
$(LOCAL_PATH)/../google/protobuf/io/zero_copy_stream_impl_lite.cc \
$(LOCAL_PATH)/../google/protobuf/map_field.cc \
$(LOCAL_PATH)/../google/protobuf/message.cc \
$(LOCAL_PATH)/../google/protobuf/message_lite.cc \
$(LOCAL_PATH)/../google/protobuf/reflection_ops.cc \
$(LOCAL_PATH)/../google/protobuf/repeated_field.cc \
$(LOCAL_PATH)/../google/protobuf/service.cc \
$(LOCAL_PATH)/../google/protobuf/source_context.pb.cc \
$(LOCAL_PATH)/../google/protobuf/struct.pb.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/atomicops_internals_x86_gcc.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/atomicops_internals_x86_msvc.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/bytestream.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/common.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/int128.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/mathlimits.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/once.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/status.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/statusor.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/stringpiece.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/stringprintf.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/structurally_valid.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/strutil.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/substitute.cc \
$(LOCAL_PATH)/../google/protobuf/stubs/time.cc \
$(LOCAL_PATH)/../google/protobuf/text_format.cc \
$(LOCAL_PATH)/../google/protobuf/timestamp.pb.cc \
$(LOCAL_PATH)/../google/protobuf/type.pb.cc \
$(LOCAL_PATH)/../google/protobuf/unknown_field_set.cc \
$(LOCAL_PATH)/../google/protobuf/util/field_comparator.cc \
$(LOCAL_PATH)/../google/protobuf/util/field_mask_util.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/datapiece.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/default_value_objectwriter.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/error_listener.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/field_mask_utility.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/json_escaping.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/json_objectwriter.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/json_stream_parser.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/object_writer.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/protostream_objectsource.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/protostream_objectwriter.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/proto_writer.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/type_info.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/type_info_test_helper.cc \
$(LOCAL_PATH)/../google/protobuf/util/internal/utility.cc \
$(LOCAL_PATH)/../google/protobuf/util/json_util.cc \
$(LOCAL_PATH)/../google/protobuf/util/message_differencer.cc \
$(LOCAL_PATH)/../google/protobuf/util/time_util.cc \
$(LOCAL_PATH)/../google/protobuf/util/type_resolver_util.cc \
$(LOCAL_PATH)/../google/protobuf/wire_format.cc \
$(LOCAL_PATH)/../google/protobuf/wire_format_lite.cc \
$(LOCAL_PATH)/../google/protobuf/wrappers.pb.cc

include $(BUILD_STATIC_LIBRARY)