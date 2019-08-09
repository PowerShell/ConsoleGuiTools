// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace OutGridView.Models
{
    public enum OutputModeOption
    {
        /// <summary>
        /// None is the default and it means OK and Cancel will not be present
        /// and no objects will be written to the pipeline.
        /// The selectionMode of the actual list will still be multiple.
        /// </summary>
        None,
        /// <summary>
        /// Allow selection of one single item to be written to the pipeline.
        /// </summary>
        Single,
        /// <summary>
        ///Allow select of multiple items to be written to the pipeline.
        /// </summary>
        Multiple
    }
}
