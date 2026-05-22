cmake_minimum_required(VERSION 3.16)

project(stb LANGUAGES C)

option(MGSTB_BUILD_SHARED "Build stb as a shared library." ON)

if(MGSTB_BUILD_SHARED)
    set(MGSTB_LIBRARY_TYPE SHARED)
else()
    set(MGSTB_LIBRARY_TYPE STATIC)
endif()

add_library(stb ${MGSTB_LIBRARY_TYPE}
    src/monogame_stb.c
)

target_include_directories(stb
    PUBLIC
        "${CMAKE_CURRENT_SOURCE_DIR}/../stb"
)

set_target_properties(stb PROPERTIES
    WINDOWS_EXPORT_ALL_SYMBOLS ON
)

if(MSVC)
    set_property(TARGET stb PROPERTY MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif()
