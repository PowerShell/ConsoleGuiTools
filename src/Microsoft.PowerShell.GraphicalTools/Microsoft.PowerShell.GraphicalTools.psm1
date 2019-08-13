#
# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

if($IsMacOS) {
    chmod +x "$PSScriptRoot/OutGridView.Gui/osx-x64/OutGridView.Gui"
} elseif ($IsLinux) {
    chmod +x "$PSScriptRoot/OutGridView.Gui/linux-x64/OutGridView.Gui"
}
