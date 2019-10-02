#
# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

if($IsMacOS) {
    chmod +x "$PSScriptRoot/GraphicalTools.Gui/osx-x64/GraphicalTools.Gui"
} elseif ($IsLinux) {
    chmod +x "$PSScriptRoot/GraphicalTools.Gui/linux-x64/GraphicalTools.Gui"
}
