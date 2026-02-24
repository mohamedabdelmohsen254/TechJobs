import { useQuery } from '@tanstack/react-query';
import { dashboardApi } from '../services/api';

export default function Dashboard() {
  const { data: stats, isLoading, error } = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: dashboardApi.getStats,
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        Error loading dashboard stats. Make sure the API is running.
      </div>
    );
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-gray-800">Dashboard</h1>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4 mb-8">
        <StatCard
          title="Total Jobs"
          value={stats?.totalJobs || 0}
          icon="💼"
          color="bg-blue-500"
        />
        <StatCard
          title="Active Jobs"
          value={stats?.activeJobs || 0}
          icon="✅"
          color="bg-green-500"
        />
        <StatCard
          title="Visible to Users"
          value={stats?.visibleJobs || 0}
          icon="👁️"
          color="bg-emerald-500"
        />
        <StatCard
          title="Hidden from Users"
          value={stats?.hiddenJobs || 0}
          icon="🙈"
          color="bg-orange-500"
        />
        <StatCard
          title="Manual Entries"
          value={stats?.manualEntries || 0}
          icon="✍️"
          color="bg-purple-500"
        />
        <StatCard
          title="Added Today"
          value={stats?.jobsAddedToday || 0}
          icon="📅"
          color="bg-pink-500"
        />
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold mb-4">Jobs by Country</h3>
          <div className="space-y-2 max-h-64 overflow-y-auto">
            {stats?.jobsByCountry && Object.entries(stats.jobsByCountry)
              .sort((a, b) => b[1] - a[1])
              .map(([country, count]) => (
              <div key={country} className="flex justify-between items-center">
                <span className="text-gray-600">{country}</span>
                <span className="bg-blue-100 text-blue-800 px-2 py-1 rounded text-sm">{count}</span>
              </div>
            ))}
            {(!stats?.jobsByCountry || Object.keys(stats.jobsByCountry).length === 0) && (
              <p className="text-gray-500">No data available</p>
            )}
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold mb-4">Jobs by Work Type</h3>
          <div className="space-y-2 max-h-64 overflow-y-auto">
            {stats?.jobsByWorkType && Object.entries(stats.jobsByWorkType)
              .sort((a, b) => b[1] - a[1])
              .map(([type, count]) => (
              <div key={type} className="flex justify-between items-center">
                <span className="text-gray-600">{type}</span>
                <span className="bg-green-100 text-green-800 px-2 py-1 rounded text-sm">{count}</span>
              </div>
            ))}
            {(!stats?.jobsByWorkType || Object.keys(stats.jobsByWorkType).length === 0) && (
              <p className="text-gray-500">No data available</p>
            )}
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-semibold mb-4">Jobs by Source</h3>
          <div className="space-y-2 max-h-64 overflow-y-auto">
            {stats?.jobsBySource && Object.entries(stats.jobsBySource)
              .sort((a, b) => b[1] - a[1])
              .map(([source, count]) => (
              <div key={source} className="flex justify-between items-center">
                <span className="text-gray-600">{source}</span>
                <span className="bg-purple-100 text-purple-800 px-2 py-1 rounded text-sm">{count}</span>
              </div>
            ))}
            {(!stats?.jobsBySource || Object.keys(stats.jobsBySource).length === 0) && (
              <p className="text-gray-500">No data available</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function StatCard({ title, value, icon, color }: { title: string; value: number; icon: string; color: string }) {
  return (
    <div className="bg-white rounded-lg shadow p-4">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-gray-500 text-xs">{title}</p>
          <p className="text-2xl font-bold text-gray-800">{value}</p>
        </div>
        <div className={`${color} p-2 rounded-full text-white text-xl`}>
          {icon}
        </div>
      </div>
    </div>
  );
}
