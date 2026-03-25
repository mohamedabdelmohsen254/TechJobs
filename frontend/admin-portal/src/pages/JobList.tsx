import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { jobsApi, GetJobsParams } from '../services/api';
import { Job } from '../types/job';

export default function JobList() {
  const queryClient = useQueryClient();
  const [params, setParams] = useState<GetJobsParams>({
    page: 1,
    pageSize: 20,
    search: '',
    isActive: undefined,
  });
  const [selectedJobs, setSelectedJobs] = useState<number[]>([]);

  const { data, isLoading, error } = useQuery({
    queryKey: ['jobs', params],
    queryFn: () => jobsApi.getJobs(params),
  });

  const deleteMutation = useMutation({
    mutationFn: jobsApi.deleteJob,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['jobs'] });
    },
  });

  const visibilityMutation = useMutation({
    mutationFn: ({ id, visible }: { id: number; visible: boolean }) => 
      jobsApi.toggleVisibility(id, visible),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['jobs'] });
    },
  });

  const bulkVisibilityMutation = useMutation({
    mutationFn: ({ jobIds, visible }: { jobIds: number[]; visible: boolean }) =>
      jobsApi.bulkToggleVisibility(jobIds, visible),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['jobs'] });
      setSelectedJobs([]);
    },
  });

  const handleDelete = async (id: number, title: string) => {
    if (window.confirm(`Are you sure you want to delete "${title}"?`)) {
      deleteMutation.mutate(id);
    }
  };

  const handleToggleVisibility = (id: number, currentVisible: boolean) => {
    visibilityMutation.mutate({ id, visible: !currentVisible });
  };

  const handleBulkVisibility = (visible: boolean) => {
    if (selectedJobs.length === 0) {
      alert('Please select jobs first');
      return;
    }
    bulkVisibilityMutation.mutate({ jobIds: selectedJobs, visible });
  };

  const handleSelectAll = () => {
    if (data?.items) {
      if (selectedJobs.length === data.items.length) {
        setSelectedJobs([]);
      } else {
        setSelectedJobs(data.items.map(j => j.id));
      }
    }
  };

  const handleSelectJob = (id: number) => {
    setSelectedJobs(prev => 
      prev.includes(id) ? prev.filter(j => j !== id) : [...prev, id]
    );
  };

  const handleSearch = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    setParams((prev) => ({
      ...prev,
      search: formData.get('search') as string,
      page: 1,
    }));
  };

  if (error) {
    return (
      <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        Error loading jobs. Make sure the API is running.
      </div>
    );
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-800">Jobs</h1>
        <Link
          to="/jobs/create"
          className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg transition-colors"
        >
          + Add Job
        </Link>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow p-4 mb-6">
        <form onSubmit={handleSearch} className="flex gap-4 items-end flex-wrap">
          <div className="flex-1 min-w-[200px]">
            <label className="block text-sm font-medium text-gray-700 mb-1">Search</label>
            <input
              type="text"
              name="search"
              defaultValue={params.search}
              placeholder="Search by title or company..."
              className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select
              value={params.isActive === undefined ? '' : params.isActive.toString()}
              onChange={(e) =>
                setParams((prev) => ({
                  ...prev,
                  isActive: e.target.value === '' ? undefined : e.target.value === 'true',
                  page: 1,
                }))
              }
              className="border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">All</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
          </div>
          <button
            type="submit"
            className="bg-gray-800 hover:bg-gray-900 text-white px-4 py-2 rounded-lg transition-colors"
          >
            Search
          </button>
        </form>
      </div>

      {/* Bulk Actions */}
      {selectedJobs.length > 0 && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-4 flex items-center justify-between">
          <span className="text-blue-800 font-medium">
            {selectedJobs.length} job{selectedJobs.length > 1 ? 's' : ''} selected
          </span>
          <div className="flex gap-2">
            <button
              onClick={() => handleBulkVisibility(true)}
              disabled={bulkVisibilityMutation.isPending}
              className="bg-green-500 hover:bg-green-600 text-white px-4 py-2 rounded-lg text-sm transition-colors disabled:opacity-50"
            >
              👁️ Show to Users
            </button>
            <button
              onClick={() => handleBulkVisibility(false)}
              disabled={bulkVisibilityMutation.isPending}
              className="bg-orange-500 hover:bg-orange-600 text-white px-4 py-2 rounded-lg text-sm transition-colors disabled:opacity-50"
            >
              🙈 Hide from Users
            </button>
            <button
              onClick={() => setSelectedJobs([])}
              className="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded-lg text-sm transition-colors"
            >
              Clear Selection
            </button>
          </div>
        </div>
      )}

      {/* Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        {isLoading ? (
          <div className="flex items-center justify-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
          </div>
        ) : (
          <>
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left">
                    <input
                      type="checkbox"
                      checked={data?.items && selectedJobs.length === data.items.length && data.items.length > 0}
                      onChange={handleSelectAll}
                      className="rounded border-gray-300"
                    />
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Job
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Company
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Location
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Source
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Visibility
                  </th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {data?.items.map((job: Job) => (
                  <tr key={job.id} className={`hover:bg-gray-50 ${!job.isVisibleToUsers ? 'bg-orange-50' : ''}`}>
                    <td className="px-4 py-4">
                      <input
                        type="checkbox"
                        checked={selectedJobs.includes(job.id)}
                        onChange={() => handleSelectJob(job.id)}
                        className="rounded border-gray-300"
                      />
                    </td>
                    <td className="px-4 py-4">
                      <div className="font-medium text-gray-900">{job.title}</div>
                      <div className="text-sm text-gray-500">{job.workType}</div>
                    </td>
                    <td className="px-4 py-4 text-gray-700">{job.company}</td>
                    <td className="px-4 py-4 text-gray-700">
                      {job.city && job.country ? `${job.city}, ${job.country}` : job.country || job.location || '-'}
                    </td>
                    <td className="px-4 py-4">
                      <span className={`px-2 py-1 text-xs rounded ${job.isManualEntry ? 'bg-purple-100 text-purple-800' : 'bg-gray-100 text-gray-800'}`}>
                        {job.source || 'Unknown'}
                      </span>
                    </td>
                    <td className="px-4 py-4">
                      <button
                        onClick={() => handleToggleVisibility(job.id, job.isVisibleToUsers)}
                        disabled={visibilityMutation.isPending}
                        className={`px-3 py-1 text-xs rounded-full font-medium transition-colors ${
                          job.isVisibleToUsers 
                            ? 'bg-green-100 text-green-800 hover:bg-green-200' 
                            : 'bg-orange-100 text-orange-800 hover:bg-orange-200'
                        }`}
                        title={job.isVisibleToUsers ? 'Click to hide from users' : 'Click to show to users'}
                      >
                        {job.isVisibleToUsers ? '👁️ Visible' : '🙈 Hidden'}
                      </button>
                    </td>
                    <td className="px-4 py-4 text-right space-x-2">
                      <a
                        href={job.applyUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-600 hover:text-blue-800"
                      >
                        View
                      </a>
                      <Link
                        to={`/jobs/${job.id}/edit`}
                        className="text-gray-600 hover:text-gray-800"
                      >
                        Edit
                      </Link>
                      <button
                        onClick={() => handleDelete(job.id, job.title)}
                        className="text-red-600 hover:text-red-800"
                        disabled={deleteMutation.isPending}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {/* Pagination */}
            {data && data.totalPages > 1 && (
              <div className="px-6 py-4 flex items-center justify-between border-t">
                <div className="text-sm text-gray-500">
                  Showing {((data.page - 1) * data.pageSize) + 1} to {Math.min(data.page * data.pageSize, data.totalCount)} of {data.totalCount} results
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => setParams((prev) => ({ ...prev, page: (prev.page || 1) - 1 }))}
                    disabled={data.page <= 1}
                    className="px-3 py-1 border rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
                  >
                    Previous
                  </button>
                  <button
                    onClick={() => setParams((prev) => ({ ...prev, page: (prev.page || 1) + 1 }))}
                    disabled={data.page >= data.totalPages}
                    className="px-3 py-1 border rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
                  >
                    Next
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
