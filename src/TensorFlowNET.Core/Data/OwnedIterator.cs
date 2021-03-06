﻿using System;
using Tensorflow.Framework.Models;

namespace Tensorflow
{
    /// <summary>
    /// An iterator producing tf.Tensor objects from a tf.data.Dataset.
    /// </summary>
    public class OwnedIterator : IteratorBase, IDisposable
    {
        IDatasetV2 _dataset;
        TensorSpec[] _element_spec;
        dataset_ops ops = new dataset_ops();
        Tensor _iterator_resource;
        Tensor _deleter;
        IteratorResourceDeleter _resource_deleter;

        public OwnedIterator(IDatasetV2 dataset)
        {
            _create_iterator(dataset);
        }

        void _create_iterator(IDatasetV2 dataset)
        {
            dataset = dataset.apply_options();
            _dataset = dataset;
            _element_spec = dataset.element_spec;
            (_iterator_resource, _deleter) = ops.anonymous_iterator_v2(_dataset.output_types, _dataset.output_shapes);
            ops.make_iterator(dataset.variant_tensor, _iterator_resource);

            // Delete the resource when this object is deleted
            _resource_deleter = new IteratorResourceDeleter(_iterator_resource, _deleter);
        }

        public Tensor[] next()
        {
            try
            {
                return ops.iterator_get_next(_iterator_resource, _dataset.output_types, _dataset.output_shapes);
            }
            catch (OutOfRangeError ex)
            {
                throw new StopIteration(ex.Message);
            }
        }

        public void Dispose()
        {
            _resource_deleter.Dispose();
        }
    }
}
